export interface AnswerOption {
  id: string
  text: string
  orderIndex: number
}

export interface Question {
  id: string
  text: string
  sectionIndex: number
  orderIndex: number
  answerOptions: AnswerOption[]
}

export interface GetQuestionnaireResponse {
  id: string
  versionNumber: string
  questions: Question[]
}

export interface AnswerSubmission {
  questionId: string
  primaryAnswerOptionId: string
  secondaryAnswerOptionId?: string
}

export interface SubmitResponseRequest {
  answers: AnswerSubmission[]
}

export interface InsightDto {
  dimensionId: string
  title: string
  text: string
  strength: number
}

export interface ReflectionGroupDto {
  id: string
  title: string
  insights: InsightDto[]
}

export interface ReflectionDto {
  groups: ReflectionGroupDto[]
}

export interface SubmitResponseResult {
  privateResultLink: string
  accessCode: string
  reflection: ReflectionDto
}

export interface GetMyReflectionResponse {
  reflection: ReflectionDto
  accessCodeAvailable: boolean
}

export interface ComparisonInsightDto {
  dimensionId: string
  title: string
  /** null = scored below display threshold for this person */
  yourStrength: number | null
  theirStrength: number | null
  /** Omitted when this person scored below threshold */
  yourText?: string
  theirText?: string
  classification: 'similarity' | 'difference'
}

export interface ComparisonGroupDto {
  id: string
  title: string
  insights: ComparisonInsightDto[]
}

/** The per-viewer comparison report (GET /api/me/comparisons/{id}). "You" = the viewer; the other
 *  person is named by `otherLabel`. No overall compatibility score, no raw answers. */
export interface ComparisonDto {
  otherLabel: string
  groups: ComparisonGroupDto[]
}

/** One row in the /me hub list (GET /api/me/comparisons). */
export interface ComparisonListItem {
  comparisonId: string
  /** The other participant's label (empty while still pending). */
  otherLabel: string
  status: 'pending' | 'complete' | 'unavailable'
  createdAt: string
}

/** The report endpoint returns either the report or a marker for a not-yet-ready comparison. */
export type ComparisonReportResponse = ComparisonDto | { state: 'pending' | 'unavailable' }

/** Result of POST /api/comparisons (US1) — the inviter mints an invite. */
export interface CreateInviteResult {
  comparisonId: string
  /** Plain single-use token; the client builds `/invite#<token>`. Never persisted plain. */
  inviteToken: string
  expiresAt: string
  /** Always "pending" for a freshly created comparison (no invitee yet). */
  status: string
}

/** Result of POST /api/invite/validate (US2) — the public face of an invite, without consuming it. */
export interface InviteValidation {
  inviterLabel: string
  /** "active" · "used" · "expired" (only "active" is returned with 200). */
  status: string
  questionnaireVersion: string
}

/** Body of POST /api/invite/join (US2). */
export interface JoinInviteRequest {
  token: string
  consent: boolean
  inviteeLabel: string
  answers: AnswerSubmission[]
}

/** Result of POST /api/invite/join (US2) — the invitee's own credentials. */
export interface JoinInviteResult {
  privateResultLink: string
  accessCode: string
  comparisonId: string
}
