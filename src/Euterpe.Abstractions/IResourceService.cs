namespace Euterpe.Abstractions;

public interface IResourceService
{
    Stream GetAssetAsStream(string fileName);
}
