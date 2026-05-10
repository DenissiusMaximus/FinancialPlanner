import React, { useMemo, useState } from 'react';
import { Modal } from './Modal';
import { Button } from './Button';
import { getCurrencyCode } from '../utils/display-helpers';

interface SourceSelectionModalProps {
  isOpen: boolean;
  onClose: () => void;
  sources: any[];
  linkedSourceIds: Set<string>;
  onConfirm: (selectedSourceIds: number[]) => void;
  isLoading?: boolean;
}

export const SourceSelectionModal: React.FC<SourceSelectionModalProps> = ({
  isOpen,
  onClose,
  sources,
  linkedSourceIds,
  onConfirm,
  isLoading = false,
}) => {
  const [selectedSourceIds, setSelectedSourceIds] = useState<Set<number>>(new Set());

  const availableSources = useMemo(
    () => sources.filter((s) => !s?.isArchived && !linkedSourceIds.has(String(s.id))),
    [sources, linkedSourceIds],
  );

  const toggleSource = (sourceId: number) => {
    const newSelected = new Set(selectedSourceIds);
    if (newSelected.has(sourceId)) {
      newSelected.delete(sourceId);
    } else {
      newSelected.add(sourceId);
    }
    setSelectedSourceIds(newSelected);
  };

  const toggleSelectAll = () => {
    if (selectedSourceIds.size === availableSources.length) {
      setSelectedSourceIds(new Set());
    } else {
      setSelectedSourceIds(new Set(availableSources.map((s) => s.id)));
    }
  };

  const handleConfirm = () => {
    onConfirm(Array.from(selectedSourceIds));
    setSelectedSourceIds(new Set());
    onClose();
  };

  const handleClose = () => {
    setSelectedSourceIds(new Set());
    onClose();
  };

  return (
    <Modal
      isOpen={isOpen}
      title="Вибір джерел"
      onClose={handleClose}
      size="md"
    >
      <div className="space-y-4">
        {availableSources.length > 0 ? (
          <>
            <p className="text-sm text-[#6b6b70] mb-4">
              Виберіть активні джерела для прив'язки до цієї цілі. Показуються тільки доступні джерела.
            </p>

            <label className="flex items-center gap-3 p-3 rounded-lg border-2 border-primary/30 bg-primary/5 hover:bg-primary/10 cursor-pointer transition-colors mb-3">
              <input
                type="checkbox"
                checked={selectedSourceIds.size === availableSources.length && availableSources.length > 0}
                onChange={toggleSelectAll}
                className="w-5 h-5 rounded border-primary border-2 text-primary bg-white focus:ring-primary/20 focus:ring-2 cursor-pointer accent-primary"
              />
              <div className="flex-1">
                <div className="font-semibold text-primary">Вибрати всі</div>
                <div className="text-xs text-primary/60">
                  {availableSources.length > 0 && `${availableSources.length} доступних джерел`}
                </div>
              </div>
            </label>

            <div className="space-y-2 max-h-[400px] overflow-y-auto pr-2">
              {availableSources.map((source) => (
                <label
                  key={source.id}
                  className="flex items-start gap-3 p-3 rounded-lg border border-hairline bg-[#fafafc] hover:bg-[#f5f5f7] cursor-pointer transition-colors"
                >
                  <input
                    type="checkbox"
                    checked={selectedSourceIds.has(source.id)}
                    onChange={() => toggleSource(source.id)}
                    className="mt-1 w-5 h-5 rounded border-hairline border-2 text-primary bg-white focus:ring-primary/20 focus:ring-2 cursor-pointer accent-primary"
                  />
                  <div className="flex-1 min-w-0">
                    <div className="flex items-center gap-2 mb-1">
                      <div className="font-medium text-ink">{source.name}</div>
                    </div>
                    <div className="text-sm text-[#6b6b70]">
                      {Number(source.amount ?? 0).toFixed(2)} {getCurrencyCode(source.currency?.name)}
                    </div>
                  </div>
                </label>
              ))}
            </div>

            <div className="pt-4 border-t border-hairline">
              <div className="text-sm text-[#6b6b70] mb-4">
                {selectedSourceIds.size > 0 ? (
                  <span className="font-medium text-ink">
                    Вибрано: {selectedSourceIds.size} {selectedSourceIds.size === 1 ? 'джерело' : 'джерел'}
                  </span>
                ) : (
                  <span>Виберіть хоча б одне джерело</span>
                )}
              </div>

              <div className="flex gap-3">
                <Button
                  variant="secondary"
                  onClick={handleClose}
                  type="button"
                  disabled={isLoading}
                >
                  Скасувати
                </Button>
                <Button
                  onClick={handleConfirm}
                  isLoading={isLoading}
                  disabled={selectedSourceIds.size === 0}
                >
                  Додати ({selectedSourceIds.size})
                </Button>
              </div>
            </div>
          </>
        ) : (
          <div className="rounded-lg border border-dashed border-hairline bg-[#fafafc] px-4 py-8 text-center text-sm text-[#6b6b70]">
            <p className="font-medium text-ink mb-1">Немає доступних джерел</p>
            <p>Усі активні джерела вже прив'язані до цієї цілі.</p>
          </div>
        )}
      </div>
    </Modal>
  );
};
