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
