namespace Euterpe.Abstractions;

public interface IMessagePackSerializationService
{
    ValueTask<Manifest> DeserializeManifestAsync(Stream stream, CancellationToken cancellationToken = default);
    ValueTask<Manifest> DeserializeManifestFromFileAsync(string filePath, CancellationToken cancellationToken = default);
    byte[] SerializeManifest(Manifest value);

    byte[] SerializeGameSharePackage(GameSharePackage value);
    GameSharePackage DeserializeGameSharePackage(byte[] bytes);
}
