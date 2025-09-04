namespace DecMailBundle.Converters;

public interface IPdfConverterFactory
{
    IPdfConverter? Create(string originalFileName);
}