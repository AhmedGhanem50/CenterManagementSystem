# Adversarial Requirements Review: Center Management System

## GAP-001: Session Cancellation Refunds
- **Scenario:** An Instructor or Admin cancels a session that students have already paid for (either as a single session or as part of a course).
- **Why it is currently undefined:** The documentation specifies that sessions can be canceled and notifications sent, but lacks financial reconciliation logic for canceled services.
- **Actors affected:** Instructor, Student, Admin
- **Current specification:** System automatically notifies Admin and Students.
- **Possible business decisions:** (a) Automatically refund to student balance, (b) Keep payment as credit for a future session, (c) Require admin intervention for manual refunds.
- **Recommended question for the business owner:** How should the system handle financial refunds or credits when an instructor cancels a session that students have already paid for?
--answer 
no action from the system the admin just can reschedule another session in the same months

## GAP-002: Student No-Shows
- **Scenario:** A student pre-pays for a single session or course but fails to attend (no QR scan).
- **Why it is currently undefined:** There is no policy defined for expired sessions where the service was available but unused.
- **Actors affected:** Student, Admin
- **Current specification:** Not specified.
- **Possible business decisions:** (a) No refund for no-shows, (b) Allow rescheduling the session once, (c) Provide a partial credit.
- **Recommended question for the business owner:** What is the financial and attendance policy for students who pre-pay for a session but fail to attend (no-show)?
--anwser
the admin can add he mannualy bye her code or id if any  


## GAP-003: Unpaid Student Access
- **Scenario:** A student attempts to scan the QR code to attend a session, but they have an unpaid or partially paid balance for the course.
- **Why it is currently undefined:** The system tracks paid/remaining amounts, but does not define if payment status blocks physical access to the session.
- **Actors affected:** Student, Admin, Instructor
- **Current specification:** QR scan checks "Current time, Group enrollment, Active session".
- **Possible business decisions:** (a) Hard block the QR scan and deny attendance, (b) Allow the scan but notify the Instructor/Admin of the unpaid status, (c) Ignore payment status for attendance purposes.
- **Recommended question for the business owner:** Should a student with an unpaid or partially paid balance be physically blocked from scanning the QR code and attending the session?
--answer
(b) Allow the scan but notify the Instructor/Admin of the unpaid status

## GAP-004: Financial Reconciliation on Student Transfer
- **Scenario:** An Admin transfers a student from Group A to Group B, where the courses have different prices, and the student has already made partial payments.
- **Why it is currently undefined:** The system creates a new enrollment and keeps payment history, but doesn't define how to calculate or migrate the financial difference.
- **Actors affected:** Student, Admin
- **Current specification:** "Keeps payment history, Creates new enrollment".
- **Possible business decisions:** (a) Automatically calculate the price difference and adjust the remaining balance, (b) Require the admin to manually close the old balance and create a new financial record.
- **Recommended question for the business owner:** When a student transfers to a new group with a different price, how should the system automatically handle price differences and previously paid amounts?
--answer 
(a) Automatically calculate the price difference and adjust the remaining balance,
## GAP-005: Incorrect Payment Entry
- **Scenario:** An Admin accidentally enters a payment of $1000 instead of $100.
- **Why it is currently undefined:** There is no defined process for data correction in financial ledgers.
- **Actors affected:** Admin, Student
- **Current specification:** Admin can "Add course payments".
- **Possible business decisions:** (a) Allow Admin to soft-delete the transaction and re-enter, (b) Require a negative "adjustment" transaction to maintain ledger integrity, (c) Require Super-Admin approval for edits.
- **Recommended question for the business owner:** How should accidental incorrect payment entries be corrected by the Admin while maintaining accurate financial audits?
--answer
(a) Allow Admin to soft-delete the transaction and re-enter,

## GAP-006: Overlapping Instructor Schedules
- **Scenario:** An Admin schedules an Instructor for two different sessions at the exact same time.
- **Why it is currently undefined:** There is no mention of race conditions or validation against scheduling conflicts.
- **Actors affected:** Admin, Instructor
- **Current specification:** Admin can "Create sessions".
- **Possible business decisions:** (a) Hard block saving the session if there's an overlap, (b) Display a warning but allow it (e.g., for joint sessions), (c) Do nothing.
- **Recommended question for the business owner:** Should the system prevent an admin from double-booking an instructor at the same time, or just display a warning?
--answer 
(a) warning and Hard block saving the session if there's an overlap,
## GAP-007: Substitute Instructors
- **Scenario:** An Instructor is suddenly unavailable for a single session, and a substitute needs to step in.
- **Why it is currently undefined:** The Admin can change instructors for a whole Group, but not temporarily for a single Session.
- **Actors affected:** Admin, Instructor
- **Current specification:** Admin can "Change instructors for groups".
- **Possible business decisions:** (a) Allow assigning a substitute instructor at the Session level, (b) Require canceling the session and creating a new one for the substitute.
- **Recommended question for the business owner:** If an instructor is absent for one day, do we need the ability to assign a substitute instructor for just that single session without changing the whole group?
--answer 
(a) Allow assigning a substitute instructor at the Session level, 
## GAP-008: Internet Disconnection During Attendance
- **Scenario:** The center's internet connection drops right as 50 students are trying to scan the QR code to enter a session.
- **Why it is currently undefined:** The system relies on real-time server checks for enrollment and time.
- **Actors affected:** Instructor, Student
- **Current specification:** Scans are processed in real-time against the database.
- **Possible business decisions:** (a) Provide an offline caching mode in a local app, (b) Instructors manually take attendance on paper and input it later, (c) Allow students to scan with their phones and sync later.
- **Recommended question for the business owner:** How should attendance be recorded if the center experiences an internet outage during a busy session check-in?
--answer
(a) Provide an offline caching mode in a local app,
## GAP-009: Accidental Session Cancellation
- **Scenario:** An Instructor or Admin clicks "Cancel Session" by mistake.
- **Why it is currently undefined:** State transitions do not define recovery after failure or reopening completed/cancelled operations.
- **Actors affected:** Admin, Instructor, Student
- **Current specification:** Session can be canceled. Notifications are sent automatically.
- **Possible business decisions:** (a) Allow "un-canceling" within a 5-minute grace period before notifications go out, (b) Require creating a brand new session, (c) Allow Admin-only un-canceling.
- **Recommended question for the business owner:** If a session is canceled by mistake, should there be a way to reverse the cancellation, and how do we handle the notifications that were already sent?
--answer
(a) Allow "un-canceling" within a 5-minute grace period before notifications go out
## GAP-010: Duplicate QR Scans
- **Scenario:** A student accidentally or intentionally scans the same session QR code multiple times.
- **Why it is currently undefined:** Concurrent actions and duplicate actions for attendance logging are not constrained.
- **Actors affected:** Student, Instructor
- **Current specification:** "Attendance recorded" upon scan.
- **Possible business decisions:** (a) Ignore subsequent scans silently, (b) Log duplicate scan attempts for auditing, (c) Throw an error to the user.
- **Recommended question for the business owner:** How should the system respond if a student scans the attendance QR code multiple times for the exact same session?
--answer
 (a) Ignore subsequent scans silently and throught a warning if if it reached 3 times ,
## GAP-011: Late QR Scan Thresholds
- **Scenario:** A session ends at 5:00 PM. A student arrives at 4:55 PM and scans the QR code.
- **Why it is currently undefined:** Status transition conflicts for partial completion of a session are not defined.
- **Actors affected:** Student, Admin
- **Current specification:** System checks "Current time".
- **Possible business decisions:** (a) Reject the scan if the session is more than 50% over, (b) Accept it but mark as "Late", (c) Accept it as "Present" as long as the session hasn't officially ended.
- **Recommended question for the business owner:** What are the exact time thresholds for marking a student as 'Present' versus 'Late', and should scans be completely rejected if they arrive too close to the end of the session?
--answer
(a) Reject the scan if the session is more than 50% over

## GAP-012: Notification Failure
- **Scenario:** The external SMS or Email provider goes down, and a session cancellation notification fails to send.
- **Why it is currently undefined:** Operational exceptions and recovery after failure for third-party integrations are missing.
- **Actors affected:** Admin, Student
- **Current specification:** "System automatically notifies Admin, Students".
- **Possible business decisions:** (a) Queue and retry indefinitely, (b) Fail silently and log the error, (c) Alert the Admin immediately in the dashboard that notifications failed.
- **Recommended question for the business owner:** If the notification service (SMS/Email) fails, should the system retry automatically, and how should it alert the Admin?
--answer
(a) Queue and retry indefinitely and Alert the Admin immediately in the dashboard that notifications failed
---

# Critical Gaps
1. **GAP-001**: Session Cancellation Refunds
2. **GAP-003**: Unpaid Student Access
3. **GAP-004**: Financial Reconciliation on Student Transfer

# High Priority Gaps
1. **GAP-005**: Incorrect Payment Entry
2. **GAP-006**: Overlapping Instructor Schedules
3. **GAP-002**: Student No-Shows

# Medium Priority Gaps
1. **GAP-008**: Internet Disconnection During Attendance
2. **GAP-009**: Accidental Session Cancellation
3. **GAP-007**: Substitute Instructors
4. **GAP-011**: Late QR Scan Thresholds

# Low Priority Gaps
1. **GAP-010**: Duplicate QR Scans
2. **GAP-012**: Notification Failure

owner additional feature 

عاوز لما اسجل الطالب يطلعلي علي طول الكارد بتاعه كادمن بال qr
بصورته لو موجودة ب لوجو المكان 
ويظهرلي خانة download 
عشان انزل الكارد ده او تربطه بالطابعة بتاعتي عشان اطبعه للطالب علي طول فور تسجيله معانا
عشان بعد كدا هو اوب ما نعمله سكان بالكارد بتاعه يتسجل وكدا   
وعاوز يكون منظر الكارد فريندلي ونظيف 

