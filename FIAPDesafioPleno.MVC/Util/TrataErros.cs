using System.Text.Json;

namespace FIAPDesafioPleno.MVC.Util
{
    public class TrataErros
    {
        public static string TrataMensagemErro(string erroContent)
        {
            try
            {
                using var doc = JsonDocument.Parse(erroContent);

                if (doc.RootElement.TryGetProperty("errors", out var errors))
                {
                    var resultado = errors.EnumerateObject()
                                          .SelectMany(p => p.Value.EnumerateArray().Select(v => v.GetString()))
                                          .Where(m => !string.IsNullOrEmpty(m))
                                          .Aggregate((string)null, (a, b) => a == null ? b : a + "<br/>" + b);

                    return resultado;
                }
            }
            catch { }

            return erroContent;
        }
    }
}
