import { singletonVideoStreamLinksListener, singletonVideoStreamLinksRemoveListener } from '../../app/createSingletonListener';
import { iptvApi } from '../../store/iptvApi';

export const enhancedApiVideoStreamLinksLocal = iptvApi.enhanceEndpoints({
  endpoints: {
    videoStreamLinksGetVideoStreamVideoStreamIds: {
      async onCacheEntryAdded(api, { dispatch, cacheDataLoaded, cacheEntryRemoved }) {
        try {
          await cacheDataLoaded;

          const updateCachedDataWithResults = () => {
            dispatch(iptvApi.util.invalidateTags(['VideoStreamLinks']));
            return;
          };

          const removeCachedDataWithResults = () => {
            dispatch(iptvApi.util.invalidateTags(['VideoStreamLinks']));
            return;
          };

          singletonVideoStreamLinksListener.addListener(updateCachedDataWithResults);
          singletonVideoStreamLinksRemoveListener.addListener(removeCachedDataWithResults);
          await cacheEntryRemoved;
          singletonVideoStreamLinksListener.removeListener(updateCachedDataWithResults);
          singletonVideoStreamLinksRemoveListener.removeListener(removeCachedDataWithResults);

        } catch (error) {
          console.error('Error in onCacheEntryAdded:', error);
        }

      }
    },
  }
});
