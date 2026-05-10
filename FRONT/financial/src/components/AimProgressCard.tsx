import React, { useState } from 'react';
import { useSortable } from '@dnd-kit/sortable';
import { CSS } from '@dnd-kit/utilities';
import { Card } from './Card';
import { useCurrencyConvert } from '../hooks/useCurrencyConvert';
import { getCurrencyDisplay } from '../utils/display-helpers';

interface AimProgress {
  id: number;
  name: string;
  amount: number;
  priority: number;
  progress: {
    collectedAmount: number;
    targetAmount: number;
    completionPercentage: number;
  };
  currency: {
    id: number;
    name: string;
    usdExchangeRate?: number;
  };
}

interface AimProgressCardProps {
  aim: AimProgress;
  onEdit?: (aim: AimProgress) => void;
  onDelete?: (id: number) => void;
  isDragging?: boolean;
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

const IconDragHandle = () => (
  <svg width="16" height="16" viewBox="0 0 24 24" fill="currentColor" className="opacity-60">
    <circle cx="9" cy="6" r="1.5" />
    <circle cx="9" cy="12" r="1.5" />
    <circle cx="9" cy="18" r="1.5" />
    <circle cx="15" cy="6" r="1.5" />
    <circle cx="15" cy="12" r="1.5" />
    <circle cx="15" cy="18" r="1.5" />
  </svg>
);

export const AimProgressCard: React.FC<AimProgressCardProps> = ({ aim, onEdit, onDelete, isDragging: externalIsDragging }) => {
  const {
    attributes,
    listeners,
    setNodeRef,
    transform,
    transition,
    isDragging: sortableDragging,
  } = useSortable({ id: aim.id });

  const isDragging = sortableDragging || externalIsDragging;
  const { collectedAmount, targetAmount, completionPercentage } = aim.progress;
  const { convert, selectedCurrencyName } = useCurrencyConvert();
  const [showActions, setShowActions] = useState(false);
  const showActionButtons = Boolean(onEdit || onDelete);

  const displayCollected = convert(collectedAmount, aim.currency);
  const displayTarget = convert(targetAmount, aim.currency);
  const displayCurrency = selectedCurrencyName ?? aim.currency?.name ?? '';
  const pct = Math.min(completionPercentage, 100);
  const isComplete = completionPercentage >= 100;

  const style = {
    transform: CSS.Translate.toString(transform),
    transition,
  };

  return (
    <div
      ref={setNodeRef}
      style={style}
      className={`flex gap-3 ${isDragging ? 'opacity-50' : ''}`}
      {...attributes}
    >
      {/* Left Section: Priority + Drag Handle */}
      <div
        className="flex flex-col items-center gap-1.5 pt-2 flex-shrink-0"
        {...listeners}
      >
        {/* Priority Badge */}
        <div className="flex items-center justify-center w-8 h-8 rounded-lg bg-primary/10 border border-primary/20">
          <span className="text-xs font-bold text-primary">{aim.priority}</span>
        </div>
        {/* Drag Handle */}
        <div className="cursor-grab active:cursor-grabbing text-primary/40 hover:text-primary/60 transition-colors">
          <IconDragHandle />
        </div>
      </div>

      {/* Main Card Content */}
      <Card
        className="flex-1 transition-all flex flex-col group"
        onMouseEnter={() => showActionButtons && setShowActions(true)}
        onMouseLeave={() => showActionButtons && setShowActions(false)}
      >
        {/* Header */}
        <div className="flex justify-between items-start min-h-[24px] mb-3">
          <h4 className="text-sm font-semibold text-ink flex-1 pr-2 line-clamp-2 leading-snug">
            {aim.name}
          </h4>
          {showActions && showActionButtons ? (
            <div className="flex gap-1 shrink-0">
              {onEdit && (
                <button
                  className="text-[#7a7a7a] hover:text-primary transition-colors p-1 rounded hover:bg-primary/5"
                  onClick={(e) => { e.stopPropagation(); onEdit(aim); }}
                  title="Редагувати"
                >
                  <IconEdit />
                </button>
              )}
              {onDelete && (
                <button
                  className="text-[#7a7a7a] hover:text-red-500 transition-colors p-1 rounded hover:bg-red-50"
                  onClick={(e) => { e.stopPropagation(); onDelete(aim.id); }}
                  title="Видалити"
                >
                  <IconTrash />
                </button>
              )}
            </div>
          ) : (
            <span className={`text-[11px] font-bold shrink-0 px-1.5 py-0.5 rounded-full ${isComplete ? 'bg-emerald-100 text-emerald-700' : 'bg-amber-50 text-amber-600'}`}>
              {isComplete ? '✓' : aim.name !== aim.name ? '' : ''}
            </span>
          )}
        </div>

        {/* Amounts */}
        <div className="flex gap-3 mb-3 text-xs">
          <div className="flex flex-col min-w-0">
            <span className="text-[#7a7a7a] mb-0.5">Зібрано</span>
            <span className="text-ink font-semibold font-mono truncate">
              {displayCollected.toFixed(2)}
              <span className="text-[#7a7a7a] font-normal ml-0.5">{displayCurrency}</span>
            </span>
          </div>
          <div className="text-[#d2d2d7] self-center">→</div>
          <div className="flex flex-col min-w-0">
            <span className="text-[#7a7a7a] mb-0.5">Мета</span>
            <span className="text-ink font-semibold font-mono truncate">
              {displayTarget.toFixed(2)}
              <span className="text-[#7a7a7a] font-normal ml-0.5">{displayCurrency}</span>
            </span>
          </div>
        </div>

        {/* Progress bar */}
        <div className="h-1.5 bg-[#f0f0f0] rounded-full overflow-hidden mb-2 mt-auto">
          <div
            className={`h-full rounded-full transition-all duration-500 ${isComplete ? 'bg-emerald-500' : 'bg-gradient-to-r from-primary to-primary-focus'}`}
            style={{ width: `${pct}%` }}
          />
        </div>

        <div className="flex justify-between items-center text-xs">
          <span className={`font-semibold ${isComplete ? 'text-emerald-600' : 'text-primary'}`}>
            {completionPercentage.toFixed(1)}%
          </span>
          <span className="text-[#7a7a7a]">{getCurrencyDisplay(aim.currency?.name)}</span>
        </div>
      </Card>
    </div>
  );
};
