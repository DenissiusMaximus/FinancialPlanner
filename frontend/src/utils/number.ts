export const toSafeNumber = (value: unknown, fallback = 0): number => {
  if (typeof value === 'number' && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === 'string' && value.trim() !== '') {
    const parsed = Number(value);
    if (Number.isFinite(parsed)) {
      return parsed;
    }
  }

  return fallback;
};

export const formatNumber = (
  value: unknown,
  options?: Intl.NumberFormatOptions
): string => {
  return toSafeNumber(value).toLocaleString(undefined, options);
};

export const formatCurrency = (
  value: unknown,
  options: Intl.NumberFormatOptions = { minimumFractionDigits: 2, maximumFractionDigits: 2 }
): string => {
  return formatNumber(value, options);
};
