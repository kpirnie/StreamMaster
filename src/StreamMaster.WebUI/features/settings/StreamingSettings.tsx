import { GetMessage } from '@lib/common/intl';
import { Fieldset } from 'primereact/fieldset';
import React, { useMemo } from 'react';
import { GetInputNumberLine } from './components/GetInputNumberLine';

import { useSettingsContext } from '@lib/context/SettingsProvider';
import useGetCommandProfiles from '@lib/smAPI/Profiles/useGetCommandProfiles';
import useGetOutputProfiles from '@lib/smAPI/Profiles/useGetOutputProfiles';
import { SelectItem } from 'primereact/selectitem';
import { BaseSettings } from './BaseSettings';
import { GetCheckBoxLine } from './components/GetCheckBoxLine';
import { GetDropDownLine } from './components/GetDropDownLine';
import { GetInputTextLine } from './components/GetInputTextLine';

export function StreamingSettings(): React.ReactElement {
  const { currentSetting } = useSettingsContext();
  const { data: commandProfiles } = useGetCommandProfiles();
  const { data: outputProfiles } = useGetOutputProfiles();

  const getIntroOptions = (): SelectItem[] => {
    var options = [
      { label: 'None', value: 0 },
      { label: 'Once', value: 1 },
      { label: 'Always', value: 2 }
    ] as SelectItem[];
    return options;
  };

  const DefaultCommandProfileNameOptions = useMemo((): SelectItem[] => {
    if (!commandProfiles) {
      return [];
    }

    const ret = commandProfiles.map(
      (x) =>
        ({
          label: x.ProfileName,
          value: x.ProfileName
        } as SelectItem)
    );
    return ret;
  }, [commandProfiles]);

  const DefaultOutputProfileNameOptions = useMemo((): SelectItem[] => {
    if (!outputProfiles) {
      return [];
    }

    const ret = outputProfiles.map(
      (x) =>
        ({
          label: x.ProfileName,
          value: x.ProfileName
        } as SelectItem)
    );
    return ret;
  }, [outputProfiles]);

  if (!currentSetting) {
    return (
      <Fieldset className="mt-4 pt-10" legend={GetMessage('SD')}>
        <div className="text-center">{GetMessage('loading')}</div>
      </Fieldset>
    );
  }

  return (
    <BaseSettings title="STREAMING">
      {GetInputNumberLine({ field: 'StreamStartTimeoutMs', max: 999999 })}
      {GetInputNumberLine({ field: 'StreamReadTimeOutMs', max: 999999 })}
      {GetInputNumberLine({ field: 'StreamRetryLimit', max: 999999 })}
      {GetInputNumberLine({ field: 'ClientReadTimeoutMs', max: 999999 })}
      {GetInputNumberLine({ field: 'StreamShutDownDelayMs', max: 999999 })}
      {GetInputNumberLine({ field: 'StreamRetryHours', max: 999999 })}

      {GetInputNumberLine({ field: 'GlobalStreamLimit' })}

      {GetInputTextLine({ field: 'ClientUserAgent' })}
      {GetCheckBoxLine({ field: 'ShowClientHostNames' })}
      {GetDropDownLine({ field: 'ShowIntros', options: getIntroOptions() })}
      {GetCheckBoxLine({ field: 'ShowMessageVideos' })}
      {GetDropDownLine({ field: 'DefaultCommandProfileName', options: DefaultCommandProfileNameOptions })}
      {GetDropDownLine({ field: 'DefaultOutputProfileName', options: DefaultOutputProfileNameOptions })}
      {/* {GetDropDownLine({ field: 'M3U8OutPutProfile', options: DefaultCommandProfileNameOptions })} */}
    </BaseSettings>
  );
}
