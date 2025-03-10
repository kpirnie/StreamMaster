import { type ReactNode } from 'react';

import { camel2title } from '@lib/common/common';
import { ColumnFieldType } from '../types/smDataTableTypes';

function getHeader(field: string, header: string | undefined, fieldType: ColumnFieldType | undefined): ReactNode {
  if (fieldType === 'custom') {
    return header || '';
  }
  if (!fieldType === undefined) {
    return header || camel2title(field);
  }

  switch (fieldType) {
    case 'blank': {
      return <div />;
    }
    case 'epg': {
      return 'EPG';
    }
    case 'm3ulink': {
      return 'M3U';
    }

    case 'epglink': {
      return 'EPG';
    }
    case 'url': {
      return 'HDHR';
    }

    case 'image': {
      return '';
    }

    case 'streams': {
      return <div>Streams</div>;
    }
    default: {
      if (header === '') {
        return '';
      }

      return header || camel2title(field);
    }
  }
}

export default getHeader;
