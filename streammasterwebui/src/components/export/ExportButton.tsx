import { Button } from "primereact/button";
import { memo } from "react";
import { getTopToolOptions } from "../../common/common";

type ExportButtonProps = {
  exportCSV: () => void;
}

const ExportButton: React.FC<ExportButtonProps> = ({ exportCSV }) => {
  return (
    <Button
      className="p-button-text justify-content-end"
      icon="pi pi-file-export"
      onClick={() => exportCSV()}
      rounded
      text
      tooltip="Export CSV"
      tooltipOptions={getTopToolOptions}
      type="button"
    />
  );
}

export default memo(ExportButton);
