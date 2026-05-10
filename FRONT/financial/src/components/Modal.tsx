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
    sm: 'w-96',
    md: 'w-[600px]',
    lg: 'w-[900px]',
  };

  return (
    <div
      className="fixed inset-0 bg-black/30 flex items-center justify-center z-[2000] animate-fadeIn"
      onClick={onClose}
      {...props}
    >
      <div
        className={`${sizeClasses[size]} max-h-[90vh] bg-white rounded-lg shadow-2xl flex flex-col slide-in-up`}
        onClick={(e) => e.stopPropagation()}
      >
        <div className="flex justify-between items-center px-6 py-6 border-b border-[#f0f0f0]">
          <h2 className="font-display text-xl font-semibold text-ink">{title}</h2>
          <button
            className="text-2xl text-[#7a7a7a] hover:text-ink transition-colors p-1"
            onClick={onClose}
          >
            ✕
          </button>
        </div>
        <div className="flex-1 overflow-y-auto px-6 py-6">{children}</div>
      </div>
    </div>
  );
};
