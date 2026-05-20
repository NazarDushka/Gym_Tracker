export interface Target {
    Id: number;
    UserId: number;
    MeasurementTypeId: number;
    Value: number;
    Date: Date | string;
    IsActive: boolean;
}
