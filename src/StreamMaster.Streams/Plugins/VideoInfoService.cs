﻿using System.Collections.Concurrent;

namespace StreamMaster.Streams.Plugins
{
    public class VideoInfoService(ILogger<VideoInfoService> logger, IDataRefreshService dataRefreshService, ILogger<VideoInfoPlugin> pluginLogger, IOptionsMonitor<Setting> intSettings)
        : IVideoInfoService
    {
        public ConcurrentDictionary<string, VideoInfo> VideoInfos { get; } = new();
        private ConcurrentDictionary<string, VideoInfoPlugin> VideoInfoPlugins { get; } = new();

        private ISourceBroadcaster? SourceBroadcaster = null;

        public void Stop()
        {
            SourceBroadcaster?.RemoveChannelBroadcaster("VideoInfo");
            foreach (VideoInfoPlugin videoInfoPlugin in VideoInfoPlugins.Values)
            {
                videoInfoPlugin.Stop();
            }
        }

        public bool HasVideoInfo(string key)
        {
            return VideoInfos.ContainsKey(key);
        }

        public VideoInfo? GetVideoInfo(string key)
        {
            return VideoInfos.TryGetValue(key, out VideoInfo? videoInfo) ? videoInfo : null;
        }

        public IEnumerable<VideoInfo> GetVideoInfos()
        {
            return VideoInfos.Values;
        }

        public void SetSourceChannel(ISourceBroadcaster sourceBroadcaster, string Id, string Name)
        {
            SourceBroadcaster = sourceBroadcaster;
            if (!VideoInfoPlugins.TryGetValue(Id, out VideoInfoPlugin? videoInfoPlugin))
            {
                logger.LogInformation("Video info service started for {Name}", Name);

                // Pass the PipeReader to the VideoInfoPlugin
                videoInfoPlugin = new VideoInfoPlugin(pluginLogger, intSettings, Id, Name);
                videoInfoPlugin.VideoInfoUpdated += OnVideoInfoUpdated;
                VideoInfoPlugins.TryAdd(Id, videoInfoPlugin);

                videoInfoPlugin.Start();
                // Add the PipeWriter to the broadcaster
                sourceBroadcaster.AddChannelBroadcaster("VideoInfo", videoInfoPlugin);
            }
        }

        public bool StopVideoPlugin(string Id)
        {
            if (VideoInfoPlugins.TryRemove(Id, out VideoInfoPlugin? videoInfoPlugin))
            {
                VideoInfos.TryRemove(Id, out _);
                videoInfoPlugin.Stop();
                return true;
            }
            return false;
        }

        private void OnVideoInfoUpdated(object? sender, VideoInfoEventArgs e)
        {
            logger.LogDebug("Video info got info for {key} {JsonOutput}", e.Id, e.VideoInfo.JsonOutput);
            VideoInfo updatedVideoInfo = e.VideoInfo;
            VideoInfos.AddOrUpdate(e.Id, updatedVideoInfo, (_, _) => updatedVideoInfo);
            dataRefreshService.RefreshVideoInfos().Wait();
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }
    }
}