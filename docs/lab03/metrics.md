# ЛР3: метрики «до» і «після»

Варіант 9, CinemaTickets. «До» — метод `LegacyProcessor.Process` (коміт d2ccdf7),
«після» — модулі `Orders` і `Reporting` бібліотеки `CinemaTickets.Core`.

## Цикломатична складність «до»

ЦС = 1 + кількість точок розгалуження (if, else if, for, &&, ||).

| Рядок | Умова | Точок |
| --- | --- | --- |
| 14 | `if (orderId <= 0)` | 1 |
| 19 | `if (viewerName == null \|\| viewerName == "")` | 2 |
| 24 | `if (viewerEmail == null \|\| !viewerEmail.Contains("@"))` | 2 |
| 29 | `if (lines == null \|\| lines.Count == 0)` | 2 |
| 34–35 | `if (status != "New" && … && status != "Cancelled")` | 4 |
| 40 | `for (int i = 0; i < lines.Count; i++)` | 1 |
| 42 | `if (lines[i].Quantity <= 0)` | 1 |
| 47 | `if (lines[i].UnitPrice < 0)` | 1 |
| 53 | `if (lines[i].Quantity >= 10)` | 1 |
| 57 | `if (isRegular)` | 1 |
| 59 | `if (total > 5000)` | 1 |
| 61 | `else if (total > 2000)` | 1 |
| 63 | `if (couponCode == "SALE10")` | 1 |
| 65 | `else if (couponCode == "MINUS200" && total > 1000)` | 2 |
| 67 | `if (createdAt.DayOfWeek == DayOfWeek.Sunday)` | 1 |
| 69 | `if (total < 1000)` | 1 |
| 71 | `if (total < 0)` | 1 |
| 74 | `if (status == "New" && total > 0)` | 2 |
| 76 | `else if (status == "Paid")` | 1 |
| 78 | `else if (status == "Cancelled")` | 1 |
| 83 | `if (printToConsole)` | 1 |
| 88 | `for (int i = 0; i < lines.Count; i++)` | 1 |
| | if та else if — 20, for — 2, && і \|\| — 8 | **30** |

ЦС = 30 + 1 = **31**.

## Підпрограми «після»

| Підпрограма | Відповідальність | Параметрів | ЦС | Рядків |
| --- | --- | --- | --- | --- |
| OrderGuards.EnsureHeaderValid | реквізити замовлення й глядача | 3 | 2 | 10 |
| OrderGuards.EnsureRequestValid | передумови всього запиту | 1 | 1 | 8 |
| OrderGuards.EnsureLineValid | передумови одного рядка | 1 | 1 | 7 |
| OrderGuards.EnsureLinesValid | непорожній набір коректних рядків | 1 | 3 | 12 |
| OrderTotalCalculator.Calculate | порядок кроків розрахунку | 1 | 2 | 14 |
| OrderTotalCalculator.SubtotalOf | сума рядків із груповою знижкою | 1 | 3 | 15 |
| OrderTotalCalculator.ViewerDiscountOf | знижка постійному глядачеві | 2 | 2 | 5 |
| OrderTotalCalculator.VolumeDiscountOf | знижка за обсягом | 1 | 3 | 13 |
| OrderTotalCalculator.CouponDiscountOf | знижка за промокодом | 2 | 4 | 11 |
| OrderTotalCalculator.SundayDiscountOf | знижка в неділю | 2 | 2 | 5 |
| OrderTotalCalculator.ServiceFeeOf | сервісний збір | 2 | 2 | 5 |
| OrderTotalCalculator.NextStatusOf | наступний стан замовлення | 2 | 4 | 6 |
| OrderReport.TextOf | текст звіту | 2 | 2 | 18 |
| OrderReportPrinter (конструктор) | прив'язка до потоку виводу | 1 | 1 | 5 |
| OrderReportPrinter.Print | запис звіту в потік | 1 | 1 | 5 |
| **Разом** | | | **33** | |

## Підсумок

| Метрика | До | Після |
| --- | --- | --- |
| Рядків у найдовшому методі | 93 | 18 |
| Максимальна ЦС одного методу | 31 | 4 |
| Сумарна ЦС модуля | 31 | 33 |
| Середня ЦС на підпрограму | 31,0 | 2,2 |
| Кількість підпрограм | 1 | 15 |
| Параметрів у точці входу | 11 | 1 |
| Максимальна глибина вкладеності | 3 | 2 |
| Магічних чисел і рядків | 14 | 0 |
| Чистих функцій | 0 | 9 |
| Звертань до консолі в бібліотеці | 13 | 0 |
| Модульних тестів | 0 | 10 |
| Вид зв'язності точки входу | процедурна | функціональна |
| Найгірший вид зчеплення | за керуванням | за даними |

Сумарна ЦС зросла з 31 до 33: кожна нова підпрограма додає одиницю базової
складності, а перевірки передумов додали гілки, яких раніше не було. Значення має
максимум на одну підпрограму: 31 неможливо тримати в голові, 4 — можна.
