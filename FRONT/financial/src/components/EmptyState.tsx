import React from 'react';

interface EmptyStateProps {
  title: string;
  description?: string;
  action?: React.ReactNode;
}

const IconEmpty = () => (
  <svg width="28" height="28" viewBox="0 0 24 24" fill="none" stroke="currentColor" strokeWidth="1.5" strokeLinecap="round" strokeLinejoin="round">
    <circle cx="12" cy="12" r="10" />
    <line x1="12" y1="8" x2="12" y2="12" />
    <line x1="12" y1="16" x2="12.01" y2="16" />
  </svg>
);

export const EmptyState: React.FC<EmptyStateProps> = ({ title, description, action }) => {
  return (
    <div style={{ width: '100%', borderRadius: '1rem', border: '1.5px dashed #e0e0e0', background: 'white', padding: '3rem 1.5rem', textAlign: 'center', boxSizing: 'border-box' }}>
      <div style={{ margin: '0 auto 1rem', display: 'flex', width: 56, height: 56, alignItems: 'center', justifyContent: 'center', borderRadius: '50%', background: '#f5f5f7', color: '#0066cc' }}>
        <IconEmpty />
      </div>
      <h3 style={{ fontWeight: 600, fontSize: '1.2rem', color: '#1d1d1f', margin: '0 0 0.5rem' }}>{title}</h3>
      {description && (
        <p style={{ color: '#7a7a7a', fontSize: '0.875rem', lineHeight: 1.6, margin: '0 auto', display: 'block', width: '100%', maxWidth: 400, whiteSpace: 'normal' }}>
          {description}
        </p>
      )}
      {action && <div style={{ marginTop: '1.5rem', display: 'flex', justifyContent: 'center' }}>{action}</div>}
    </div>
  );
};