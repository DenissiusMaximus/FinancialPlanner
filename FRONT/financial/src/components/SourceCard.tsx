import React from 'react';
import { Card } from './Card';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';
import { getCurrencyCode } from '../utils/display-helpers';

interface Source {
  id: number;
  name: string;
  amount: number;
  currency: {
    id: number;
    name: string;
    usdExchangeRate: number;
  };
}

interface SourceCardProps {
  source: Source;
  onEdit?: (source: Source) => void;
  onDelete?: (id: number) => void;
  compact?: boolean;
  onClick?: () => void;
}

const IconEdit = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <path d="M11 4H4a2 2 0 0 0-2 2v14a2 2 0 0 0 2 2h14a2 2 0 0 0 2-2v-7" />
    <path d="M18.5 2.5a2.121 2.121 0 0 1 3 3L12 15l-4 1 1-4 9.5-9.5z" />
  </svg>
);

const IconTrash = () => (
  <svg width="14" height="14" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round">
    <polyline points="3 6 5 6 21 6" /><path d="M19 6l-1 14a2 2 0 0 1-2 2H8a2 2 0 0 1-2-2L5 6" />
    <path d="M10 11v6" /><path d="M14 11v6" /><path d="M9 6V4a1 1 0 0 1 1-1h4a1 1 0 0 1 1 1v2" />
  </svg>
);

export const SourceCard: React.FC<SourceCardProps> = ({ source, onEdit, onDelete, compact = false, onClick }) => {
  const [showActions, setShowActions] = React.useState(false);
  const showActionButtons = Boolean(onEdit || onDelete);
  const { convert, selectedCurrencyName } = useCurrencyConvert();

  const displayAmount = convert(source.amount ?? 0, source.currency);
  const isConverted = selectedCurrencyName?.toUpperCase() !== source.currency?.name?.toUpperCase();

  return (
    <Card
      className={`h-full w-full flex flex-col !transform-none shrink-0 ${onClick ? 'cursor-pointer' : 'cursor-default'} ${compact ? 'p-4 min-w-[200px]' : ''}`}
      onMouseEnter={() => showActionButtons && setShowActions(true)}
      onMouseLeave={() => showActionButtons && setShowActions(false)}
      onClick={onClick}
    >
      <div className={`flex justify-between items-start ${compact ? 'mb-2' : 'mb-3'}`}>
        <div className="flex-1">
          <h3 className={`${compact ? 'text-sm' : 'text-base'} font-semibold text-ink mb-1 line-clamp-1`}>{source.name}</h3>
          <span className="text-xs text-[#7a7a7a] bg-[#f5f5f7] px-2 py-0.5 rounded inline-block">
            {getCurrencyCode(source.currency?.name)}
          </span>
        </div>
        {showActionButtons && (
          <div className={`flex gap-1 ${showActions ? 'opacity-100' : 'opacity-0 sm:opacity-0 opacity-100'}`}>
            {/* Always visible on mobile (touch), hover-visible on desktop */}
            <div className="flex gap-1 sm:hidden">
              {onEdit && (
                <button
                  className="text-[#7a7a7a] hover:text-primary transition-colors p-2 rounded-lg hover:bg-primary/5 touch-manipulation"
                  onClick={(e) => { e.stopPropagation(); onEdit(source); }}
                  title="Редагувати"
                >
                  <IconEdit />
                </button>
              )}
              {onDelete && (
                <button
                  className="text-[#7a7a7a] hover:text-red-500 transition-colors p-2 rounded-lg hover:bg-red-50 touch-manipulation"
                  onClick={(e) => { e.stopPropagation(); onDelete(source.id); }}
                  title="Видалити"
                >
                  <IconTrash />
                </button>
              )}
            </div>
            {/* Desktop: show on hover */}
            {showActions && (
              <div className="hidden sm:flex gap-1">
                {onEdit && (
                  <button
                    className="text-[#7a7a7a] hover:text-primary transition-colors p-1.5 rounded-lg hover:bg-primary/5"
                    onClick={() => onEdit(source)}
                    title="Редагувати"
                  >
                    <IconEdit />
                  </button>
                )}
                {onDelete && (
                  <button
                    className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1.5 rounded-lg hover:bg-red-50"
                    onClick={() => onDelete(source.id)}
                    title="Видалити"
                  >
                    <IconTrash />
                  </button>
                )}
              </div>
            )}
          </div>
        )}
      </div>
      <div className={`font-mono ${compact ? 'text-xl' : 'text-3xl'} font-semibold text-primary truncate mt-auto`}>
        {displayAmount.toFixed(2)}
        <span className={`${compact ? 'text-sm' : 'text-base'} font-normal ml-1`}>{selectedCurrencyName}</span>
      </div>
      {isConverted && (
        <div className="text-xs text-[#7a7a7a] mt-1 truncate">
          {(source.amount ?? 0).toFixed(2)} {getCurrencyCode(source.currency?.name)}
        </div>
      )}
    </Card>
  );
};
