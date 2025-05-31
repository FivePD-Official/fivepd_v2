import { FPriority } from "@enums";

export function getPriorityLabel(priority: FPriority) {
  switch (priority) {
    case FPriority.Code1:
      return 'LABEL.PRIORITY.CODE1';
    case FPriority.Code2:
      return 'LABEL.PRIORITY.CODE2';
    case FPriority.Code3:
      return 'LABEL.PRIORITY.CODE3';
    case FPriority.Code99:
      return 'LABEL.PRIORITY.CODE99';
  }
}

export function getPriorityBadgeColor(priority: FPriority) {
  switch (priority) {
    case FPriority.Code1:
      return 'blue';
    case FPriority.Code2:
      return 'amber';
    case FPriority.Code3:
      return 'red';
    case FPriority.Code99:
      return 'red';
  }
}
