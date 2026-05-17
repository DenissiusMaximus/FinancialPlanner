import React from 'react';

interface DashboardSectionProps {
  title: string;
  children: React.ReactNode;
  action?: React.ReactNode;
  subtitle?: string;
}

export const DashboardSection: React.FC<DashboardSectionProps> = ({
  title,
  children,
  action,
  subtitle,
}) => {
  return (
    <section className="mb-10 slide-in-up">
      <div className="flex flex-wrap gap-y-2 justify-between items-start mb-5">
        <div className="min-w-0">
          <h2 className="font-display text-xl font-semibold text-ink tracking-tight">{title}</h2>
          {subtitle && <p className="text-sm text-[#7a7a7a] mt-0.5">{subtitle}</p>}
        </div>
        {action && <div className="flex gap-2 flex-wrap items-center ml-4">{action}</div>}
      </div>
      <div className="flex flex-col gap-4 w-full" style={{ alignItems: 'stretch' }}>{children}</div>
    </section>
  );
};
