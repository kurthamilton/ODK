import { DataType } from '../data-types/data-type';

export interface ChapterProperty {
    dataType: DataType;
    helpText: string;
    hidden: boolean;
    id: string;
    label: string;
    name: string;
    required: boolean;
    subtitle: string;
}
