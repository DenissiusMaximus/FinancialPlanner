import React from 'react';

interface CardProps extends React.HTMLAttributes<HTMLDivElement> {
  children: React.ReactNode;
  featured?: boolean;
}

export const Card: React.FC<CardProps> = ({
  children,
  className = '',
  featured = false,
  ...props
}) => {
  // DESIGN.md: store-utility-card = white bg, 1px hairline border, rounded.lg (18px), padding 24px
  // No shadows on cards — elevation comes from surface-color contrast only
  const baseStyles = 'rounded-[18px] p-5 border border-hairline transition-all';

  const featuredStyles = featured
    ? 'bg-surface-tile-1 text-white border-0'
    : 'bg-white hover:border-[#c8c8cc]';

  return (
    <div
      className={`${baseStyles} ${featuredStyles} ${props.onClick ? 'cursor-pointer active:scale-[0.99]' : ''} ${className}`}
      role={props.onClick ? 'button' : undefined}
      tabIndex={props.onClick ? 0 : undefined}
      {...props}
    >
      {children}
    </div>
  );
};
