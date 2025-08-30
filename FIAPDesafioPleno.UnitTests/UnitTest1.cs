namespace FIAPDesafioPleno.UnitTests
{
    public class UnitTest1
    {
        [Fact]
        public void Soma_2Mais2_Retorna4()
        {
            int resultado = 2 + 2;
            Assert.Equal(4, resultado);
        }
    }
}