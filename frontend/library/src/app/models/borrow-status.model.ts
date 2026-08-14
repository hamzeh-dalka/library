// Mirrors library.Enums.BorrowStatus on the backend exactly (numeric values
// included). The API has no JsonStringEnumConverter configured, so
// System.Text.Json serializes this as its raw underlying int (1-4), never as
// the enum name and never starting at 0.
export enum BorrowStatus {
  Borrowed = 1,
  Returned = 2,
  Overdue = 3,
  Cancelled = 4
}

export function isActiveBorrow(status: BorrowStatus | number): boolean {
  return status === BorrowStatus.Borrowed || status === BorrowStatus.Overdue;
}

export function borrowStatusLabel(status: BorrowStatus | number): string {
  switch (status) {
    case BorrowStatus.Borrowed: return 'Borrowed';
    case BorrowStatus.Returned: return 'Returned';
    case BorrowStatus.Overdue: return 'Overdue';
    case BorrowStatus.Cancelled: return 'Cancelled';
    default: return 'Unknown';
  }
}
