using Adapter.OutBound.SqlAdapter.Context;
using Adapter.OutBound.SqlAdapter.Repository;
using Domain.Models.Xml;
using Microsoft.EntityFrameworkCore;
using Moq;
using Moq.EntityFrameworkCore;
using Xunit;

namespace Unit.Repository
{
    [Trait("Category", "Unit")]
    [Trait("Component", "XmlRepository")]
    public class XmlRepositoryTests
    {
        private readonly Mock<IServiceProvider> _serviceProvider;
        private readonly Mock<SqlContext> _context;
        private readonly XmlRepository _repository;

        public XmlRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<SqlContext>()
                .Options;

            _context = new Mock<SqlContext>(options);

            _serviceProvider = new Mock<IServiceProvider>();

            _serviceProvider
                .Setup(x => x.GetService(typeof(SqlContext)))
                .Returns(_context.Object);

            _repository = new XmlRepository(
                _serviceProvider.Object);
        }

        [Fact]
        [Trait("Feature", "Construction")]
        public void CanConstruct()
        {
            Assert.NotNull(_repository);

            _serviceProvider.Verify(
                x => x.GetService(typeof(SqlContext)),
                Times.Once);
        }

        [Fact]
        [Trait("Feature", "ArgumentValidation")]
        public void CannotConstructWithNullServiceProvider()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new XmlRepository(null!));
        }

        [Fact]
        [Trait("Feature", "DependencyResolution")]
        public void CannotConstructWithoutRegisteredSqlContext()
        {
            var serviceProvider = new Mock<IServiceProvider>();

            Assert.Throws<InvalidOperationException>(() =>
                new XmlRepository(serviceProvider.Object));
        }

        [Fact]
        [Trait("Feature", "ReadTop10")]
        public async Task CanCallReadTop10()
        {
            var expected = CreateXml(1);

            _context
                .Setup(x => x.Xmls)
                .ReturnsDbSet(new List<Xml> { expected });

            var result = await _repository.ReadTop10();

            var actual = Assert.Single(result);

            Assert.Equal(expected.Id, actual.Id);

            Assert.Equal(
                expected.TipoMovimento,
                actual.TipoMovimento);

            Assert.Equal(expected.TipoNI, actual.TipoNI);
            Assert.Equal(expected.NI, actual.NI);

            Assert.Equal(
                expected.AnoMesCaixa,
                actual.AnoMesCaixa);

            Assert.Equal(
                expected.NumeroRecibo,
                actual.NumeroRecibo);
        }

        [Fact]
        [Trait("Feature", "ReadTop10")]
        public async Task ReadTop10ReturnsEmptyListWhenThereAreNoRecords()
        {
            _context
                .Setup(x => x.Xmls)
                .ReturnsDbSet(new List<Xml>());

            var result = await _repository.ReadTop10();

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Theory]
        [InlineData(1, 1)]
        [InlineData(5, 5)]
        [InlineData(10, 10)]
        [InlineData(15, 10)]
        [Trait("Feature", "ReadTop10")]
        public async Task ReadTop10ReturnsExpectedNumberOfRecords(
            int totalRecords,
            int expectedCount)
        {
            var records = Enumerable.Range(1, totalRecords)
                .Select(CreateXml)
                .ToList();

            _context
                .Setup(x => x.Xmls)
                .ReturnsDbSet(records);

            var result = await _repository.ReadTop10();

            Assert.Equal(expectedCount, result.Count);

            var expectedIds = records
                .Select(xml => xml.Id)
                .ToHashSet();

            Assert.All(
                result,
                xml => Assert.Contains(xml.Id, expectedIds));
        }

        [Fact]
        [Trait("Feature", "ExceptionPropagation")]
        public async Task ReadTop10PropagatesDependencyException()
        {
            var expected = new InvalidOperationException(
                "Falha simulada ao acessar os XMLs.");

            _context
                .Setup(x => x.Xmls)
                .Throws(expected);

            var actual = await Assert
                .ThrowsAsync<InvalidOperationException>(
                    () => _repository.ReadTop10());

            Assert.Same(expected, actual);
        }

        private static Xml CreateXml(int index)
        {
            return new Xml
            {
                Id = index.ToString(),
                TipoMovimento = "1",
                TipoNI = "1",
                NI = "12345678901",
                AnoMesCaixa = "202611",
                NumeroRecibo = $"RECIBO-{index}"
            };
        }
    }
}