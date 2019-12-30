export class DateUtils {
  static addDays(date: Date, days: number): Date {
    if (!date) {
      return date;
    }
    
    date = new Date(date.valueOf());
    date.setDate(date.getDate() + days);
    return date;
  }

  static compare(date1: Date, date2: Date): number {
    if (!date1 || !date2) {
      return date1 ? -1 : date2 ? 1 : 0;
    }

    const result: number = date1.getTime() - date2.getTime();
    return result < 0 ? -1 : result > 0 ? 1 : 0;
  }
  
  static daysBetween(date1: Date, date2: Date): number {
    const difference: number = date2.getTime() - date1.getTime();   
    return difference / (1000 * 3600 * 24); 
  }   

  static toDate(date: Date): Date {
    if (!date) {
      return date;
    }

    return new Date(date.getFullYear(), date.getMonth(), date.getDate())
  }

  static today(): Date {
    return this.toDate(new Date());
  }

  static toISODateString(date: Date): string {
    return date ? date.toISOString().slice(0, 10) : '';
  }
}