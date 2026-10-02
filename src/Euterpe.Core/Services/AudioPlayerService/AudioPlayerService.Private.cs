using Avalonia.Threading;
using SoundFlow.Abstracts.Devices;
using SoundFlow.Components;
using SoundFlow.Providers;
using SoundFlow.Structs;

namespace Euterpe.Core;

internal sealed partial class AudioPlayerService
{
    private SoundPlayer CreatePlayer(byte[] audio)
    {
        var stream = new StreamDataProvider(Engine, new MemoryStream(audio, false));
        var format = new AudioFormat
        {
            Format = stream.SampleFormat,
            Channels = stream.FormatInfo!.ChannelCount,
            Layout = AudioFormat.GetLayoutFromChannels(stream.FormatInfo.ChannelCount),
            SampleRate = stream.SampleRate
        };

        return new SoundPlayer(Engine, format, new ResilientSoundDataProvider(stream, Logger));
    }

    private void Activate(SoundPlayer player)
    {
        _player = player;

        // PlaybackEnded fires on the audio render thread; a player stopped or replaced since then is ignored.
        player.PlaybackEnded += (_, _) => Dispatcher.UIThread.Post(() =>
        {
            if (_player == player)
            {
                Stop();
            }
        });

        EnsureDevice(player.Format).MasterMixer.AddComponent(player);
        if (PlaybackState.Status is PlaybackStatus.Playing)
        {
            player.Play();
        }

        Logger.LogInformation("Playing audio {PlayingKey}", PlaybackState.PlayingKey);
    }

    private AudioPlaybackDevice EnsureDevice(AudioFormat format)
    {
        if (_device is { } existing && existing.Format == format)
        {
            return existing;
        }

        _device?.Dispose();
        _device = Engine.InitializePlaybackDevice(null, format);
        _device.Start();
        return _device;
    }

    private void StopPlayer()
    {
        if (_player is null)
        {
            return;
        }

        _player.Stop();
        _device?.MasterMixer.RemoveComponent(_player);
        _player.Dispose();
        _player = null;
    }
}
