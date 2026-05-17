import React, { useEffect } from 'react';

interface ModalProps extends React.HTMLAttributes<HTMLDivElement> {
  isOpen: boolean;
  title: string;
  onClose: () => void;
  children: React.ReactNode;
  size?: 'sm' | 'md' | 'lg';
}

export const Modal: React.FC<ModalProps> = ({
  isOpen,
  title,
  onClose,
  children,
  size = 'md',
  ...props
}) => {
  useEffect(() => {
    if (isOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'auto';
    }

    return () => {
      document.body.style.overflow = 'auto';
    };
  }, [isOpen]);

  if (!isOpen) return null;

  // Desktop: match original fixed widths. Mobile: always full-width bottom sheet.
  const desktopWidthClass = {
    sm: 'sm:w-96',
    md: 'sm:w-[600px]',
    lg: 'sm:w-[900px]',
  }[size];

  return (
    <div
      className="fixed inset-0 bg-black/30 flex items-end sm:items-center justify-center z-[2000] animate-fadeIn"
      onClick={onClose}
      {...props}
    >
      <div
        className={`w-full ${desktopWidthClass} max-h-[90vh] sm:max-h-[85vh] bg-white rounded-t-2xl sm:rounded-2xl shadow-2xl flex flex-col slide-in-up`}
        onClick={(e) => e.stopPropagation()}
      >
        {/* Header */}
        <div className="flex justify-between items-center px-5 py-4 sm:px-6 sm:py-5 border-b border-[#f0f0f0] shrink-0">
          <h2 className="font-display text-lg sm:text-xl font-semibold text-ink">{title}</h2>
          <button
            className="text-[#7a7a7a] hover:text-ink transition-colors p-2 -mr-1 touch-manipulation rounded-lg hover:bg-[#f5f5f7]"
            onClick={onClose}
            aria-label="Закрити"
          >
            <svg width="18" height="18" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="2.5" strokeLinecap="round">
              <line x1="18" y1="6" x2="6" y2="18"/><line x1="6" y1="6" x2="18" y2="18"/>
            </svg>
          </button>
        </div>
        {/* Scrollable content */}
        <div className="flex-1 overflow-y-auto px-5 py-5 sm:px-6 sm:py-6">
          {children}
        </div>
      </div>
    </div>
  );
};
