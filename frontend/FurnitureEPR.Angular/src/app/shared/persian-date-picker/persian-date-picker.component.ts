import { CommonModule } from '@angular/common';
import { Component, EventEmitter, Input, OnChanges, Output } from '@angular/core';

interface JalaliParts { year: number; month: number; day: number; }

@Component({
  selector: 'app-persian-date-picker',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './persian-date-picker.component.html',
  styleUrl: './persian-date-picker.component.scss'
})
export class PersianDatePickerComponent implements OnChanges {
  @Input() value = '';
  @Input() label = 'انتخاب تاریخ';
  @Output() valueChange = new EventEmitter<string>();

  readonly weekdays = ['ش', 'ی', 'د', 'س', 'چ', 'پ', 'ج'];
  readonly monthNames = ['فروردین', 'اردیبهشت', 'خرداد', 'تیر', 'مرداد', 'شهریور', 'مهر', 'آبان', 'آذر', 'دی', 'بهمن', 'اسفند'];
  readonly digits = new Intl.NumberFormat('fa-IR');
  readonly partsFormatter = new Intl.DateTimeFormat('en-US-u-ca-persian-nu-latn', {
    year: 'numeric', month: 'numeric', day: 'numeric', timeZone: 'UTC'
  });

  isOpen = false;
  viewYear = 1405;
  viewMonth = 1;
  days: (number | null)[] = [];

  ngOnChanges(): void {
    const date = this.value ? this.parseIso(this.value) : new Date();
    const parts = this.toJalali(date);
    this.viewYear = parts.year;
    this.viewMonth = parts.month;
    this.buildDays();
  }

  get displayValue(): string {
    if (!this.value) return '';
    const parts = this.toJalali(this.parseIso(this.value));
    return `${this.digits.format(parts.year)}/${this.digits.format(parts.month).padStart(2, '۰')}/${this.digits.format(parts.day).padStart(2, '۰')}`;
  }

  get monthTitle(): string {
    return `${this.monthNames[this.viewMonth - 1]} ${this.digits.format(this.viewYear)}`;
  }

  toggle(): void { this.isOpen = !this.isOpen; }

  previousMonth(): void {
    if (this.viewMonth === 1) { this.viewMonth = 12; this.viewYear--; }
    else this.viewMonth--;
    this.buildDays();
  }

  nextMonth(): void {
    if (this.viewMonth === 12) { this.viewMonth = 1; this.viewYear++; }
    else this.viewMonth++;
    this.buildDays();
  }

  choose(day: number | null): void {
    if (!day) return;
    const date = this.fromJalali(this.viewYear, this.viewMonth, day);
    const iso = `${date.getUTCFullYear()}-${String(date.getUTCMonth() + 1).padStart(2, '0')}-${String(date.getUTCDate()).padStart(2, '0')}`;
    this.valueChange.emit(iso);
    this.isOpen = false;
  }

  clear(event: MouseEvent): void {
    event.stopPropagation();
    this.valueChange.emit('');
  }

  private buildDays(): void {
    const first = this.fromJalali(this.viewYear, this.viewMonth, 1);
    const nextMonth = this.viewMonth === 12
      ? this.fromJalali(this.viewYear + 1, 1, 1)
      : this.fromJalali(this.viewYear, this.viewMonth + 1, 1);
    const count = Math.round((nextMonth.getTime() - first.getTime()) / 86400000);
    const offset = (first.getUTCDay() + 1) % 7; // هفته شمسی از شنبه آغاز می‌شود.
    this.days = [...Array(offset).fill(null), ...Array.from({ length: count }, (_, i) => i + 1)];
    while (this.days.length < 35) this.days.push(null);
  }

  private toJalali(date: Date): JalaliParts {
    const values: Record<string, string> = {};
    for (const part of this.partsFormatter.formatToParts(date)) values[part.type] = part.value;
    return { year: Number(values['year']), month: Number(values['month']), day: Number(values['day']) };
  }

  private fromJalali(year: number, month: number, day: number): Date {
    // آغاز سال شمسی نزدیک ۲۰ مارس است؛ پیمایش حداکثر ۴۰۰ روز، تبدیل دقیق و بدون وابستگی خارجی را فراهم می‌کند.
    const start = Date.UTC(year + 621, 2, 18);
    for (let offset = 0; offset <= 400; offset++) {
      const candidate = new Date(start + offset * 86400000);
      const parts = this.toJalali(candidate);
      if (parts.year === year && parts.month === month && parts.day === day) return candidate;
    }
    throw new Error('تاریخ شمسی معتبر نیست.');
  }

  private parseIso(value: string): Date {
    const [year, month, day] = value.slice(0, 10).split('-').map(Number);
    return new Date(Date.UTC(year, month - 1, day));
  }
}
