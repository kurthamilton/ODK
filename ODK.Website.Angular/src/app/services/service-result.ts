export interface ServiceResult<T> {
    messages?: string[];
    success: boolean;
    value?: T;
}
