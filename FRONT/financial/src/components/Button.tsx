import React from 'react';

interface ButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary' | 'tertiary' | 'danger';
  size?: 'sm' | 'md' | 'lg';
  children: React.ReactNode;
  isLoading?: boolean;
}

export const Button: React.FC<ButtonProps> = ({
  variant = 'primary',
  size = 'md',
  children,
  isLoading = false,
  disabled,
  className = '',
  ...props
}) => {
  // Design system: button-primary = bg-primary, rounded-pill, active:scale-95
  // button-secondary-pill = transparent, primary text, primary border, rounded-pill
  const baseStyles =
    'font-text transition-all rounded-full cursor-pointer inline-flex items-center justify-center gap-2 ' +
    'disabled:opacity-40 disabled:cursor-not-allowed active:scale-95 select-none whitespace-nowrap';

  const variants = {
    primary:
      'bg-primary text-white hover:bg-primary-focus border-0',
    secondary:
      'bg-transparent text-primary border border-primary hover:bg-primary/5',
    tertiary:
      'bg-[#fafafc] text-[#333333] border border-[#f0f0f0] hover:bg-[#f5f5f7]',
    danger:
      'bg-transparent text-red-600 border border-red-300 hover:bg-red-50',
  };

  // Design: body text 17px / font-weight 400 for primary large
  const sizes = {
    sm:  'px-[14px] py-[7px]  text-[13px] leading-none',
    md:  'px-[18px] py-[10px] text-[14px] leading-none',
    lg:  'px-[22px] py-[11px] text-[17px] leading-none',
  };

  return (
    <button
      className={`${baseStyles} ${variants[variant]} ${sizes[size]} ${className}`}
      disabled={disabled || isLoading}
      {...props}
    >
      {isLoading ? (
        <span className="flex items-center gap-1.5">
          <svg className="animate-spin h-3.5 w-3.5" viewBox="0 0 24 24" fill="none">
            <circle className="opacity-25" cx="12" cy="12" r="10" stroke="currentColor" strokeWidth="3" />
            <path className="opacity-75" fill="currentColor" d="M4 12a8 8 0 018-8v4l3-3-3-3v4a8 8 0 00-8 8h4z" />
          </svg>
          {children}
        </span>
      ) : children}
    </button>
  );
};
