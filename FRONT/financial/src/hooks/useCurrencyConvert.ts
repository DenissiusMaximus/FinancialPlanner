import { useGetApiCurrency } from '../api/generated/endpoints';
import { useCurrencyStore } from '../store/currencyStore';

interface CurrencyLike {
  name?: string | null;
  usdExchangeRate?: number | null;
}

/**
 * Converts amount from one currency to another using usdExchangeRate.
 * usdExchangeRate = how many USD is 1 unit of this currency.
 *   e.g. UAH: 0.0233 (1 UAH = 0.0233 USD)
 *        USD: 1.0
 *        EUR: 1.18 (1 EUR = 1.18 USD)
 *
 * Formula:
 *   amount_in_usd    = amount * fromRate
 *   amount_in_target = amount_in_usd / toRate
 */
export function convertAmount(
  amount: number,
  fromCurrency: CurrencyLike | undefined | null,
  toCurrency: CurrencyLike | undefined | null,
): number {
  const fromRate = fromCurrency?.usdExchangeRate ?? 1;
  const toRate = toCurrency?.usdExchangeRate ?? 1;
  if (fromRate === 0 || toRate === 0) return amount;
  const amountInUsd = amount * fromRate;
  return amountInUsd / toRate;
}

/**
 * Hook that provides currency conversion utilities.
 * Returns the selected currency object and a convert function.
 */
export function useCurrencyConvert() {
  const selectedCurrencyName = useCurrencyStore((s) => s.selectedCurrency);
  const currenciesQuery = useGetApiCurrency();
  const currencies = (Array.isArray(currenciesQuery.data) ? currenciesQuery.data : []) as CurrencyLike[];

  const selectedCurrencyObj = currencies.find(
    (c) => c.name?.toUpperCase() === selectedCurrencyName?.toUpperCase()
  ) ?? null;

  const convert = (amount: number, fromCurrency: CurrencyLike | undefined | null): number => {
    if (!selectedCurrencyObj || !fromCurrency) return amount;
    if (fromCurrency.name?.toUpperCase() === selectedCurrencyName?.toUpperCase()) return amount;
    return convertAmount(amount, fromCurrency, selectedCurrencyObj);
  };

  return {
    currencies,
    selectedCurrencyName,
    selectedCurrencyObj,
    convert,
    isLoading: currenciesQuery.isLoading,
  };
}
