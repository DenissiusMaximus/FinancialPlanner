import { create } from 'zustand';
import { devtools } from 'zustand/middleware';

interface UIStore {
  expandedAims: boolean;
  setExpandedAims: (expanded: boolean) => void;
}

export const useUIStore = create<UIStore>()(
  devtools((set) => ({
    expandedAims: false,
    setExpandedAims: (expanded: boolean) => set({ expandedAims: expanded }),
  }))
);
