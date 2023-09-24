namespace BirdNames.Core.Interfaces;

public interface IBirdNamesXmlProcessorService: IDisposable, IAsyncDisposable
{
  Task ProcessXml(Stream xmlSource);
}