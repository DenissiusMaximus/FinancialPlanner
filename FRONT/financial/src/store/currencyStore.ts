import { create } from 'zustand';
import { createJSONStorage, devtools, persist } from 'zustand/middleware';

interface CurrencyStore {
  selectedCurrency: string;
  setSelectedCurrency: (currencyCode: string) => void;
}

export const useCurrencyStore = create<CurrencyStore>()(
  devtools(
    persist(
      (set) => ({
        selectedCurrency: 'UAH',
        setSelectedCurrency: (currencyCode: string) =>
          set({ selectedCurrency: currencyCode }),
      }),
      {
        name: 'currency-storage',
        storage: createJSONStorage(() => localStorage),
      }
    )
  )
);
