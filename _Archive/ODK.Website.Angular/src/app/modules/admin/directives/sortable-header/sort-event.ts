import { SortDirection } from './sort-direction';

export interface SortEvent {
  column: string;
  direction: SortDirection;
}
