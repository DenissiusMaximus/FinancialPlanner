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

  const sizeClasses = {
    sm: 'w-full max-w-sm',
    md: 'w-full max-w-xl',
    lg: 'w-full max-w-4xl',
  };

  return (
    <div
      className="fixed inset-0 bg-black/30 flex items-end sm:items-center justify-center z-[2000] animate-fadeIn p-0 sm:p-4"
      onClick={onClose}
      {...props}
    >
      <div
        className={`${sizeClasses[size]} max-h-[92vh] sm:max-h-[90vh] bg-white rounded-t-2xl sm:rounded-2xl shadow-2xl flex flex-col slide-in-up w-full`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center px-5 py-4 sm:px-6 sm:py-5 border-b border-[#f0f0f0] shrink-0">
          <h2 className="font-display text-lg sm:text-xl font-semibold text-ink">{title}</h2>
          <button
            className="text-2xl text-[#7a7a7a] hover:text-ink transition-colors p-2 -mr-1 touch-manipulation"
            onClick={onClose}
          >
            ✕
          </button>
        </div>
        <div className="flex-1 overflow-y-auto px-5 py-5 sm:px-6 sm:py-6">{children}</div>
      </div>
    </div>
  );
};
