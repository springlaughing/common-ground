# Questionary
We are making something stronger than a "software methodology quiz." It is closer to:
•	collaboration expectations 
•	operational trust 
•	planning behavior 
•	communication norms 
•	conflict/friction patterns 
•	quality philosophy 
•	psychological safety 
•	delivery style 
…with software/product work as the primary environment.

Choose one primary answer. You may also choose one secondary answer.

## Section 1: Collaboration setup

**Q1 When starting work with someone new, what helps you feel ready to collaborate?**

A. Written goals, context, and expectations — I need to understand what we are working toward before I can contribute. A document I can return to is more reliable than a conversation I have to remember.

B. A live conversation to ask questions and calibrate — written context only tells me what someone decided to write down. A real exchange lets me test my understanding and surface what I did not know I needed to ask.

C. Seeing examples or trying a small piece of real work — I learn more about how a collaboration works by doing something together than by discussing it first.

D. Clear ownership and decision boundaries outlined somewhere, and discussion about them when needed — I can move quickly once I know who is accountable for what and where my judgment applies.

Hidden mapping:
- A: upfront_clarity_need: +2, clarity_via_written_context: +2, verbal_alignment_preference: -1
- B: upfront_clarity_need: +2, verbal_alignment_preference: +2, clarity_via_written_context: -1
- C: learning_by_doing: +2, ambiguity_tolerance: +2, upfront_clarity_need: -1
- D: ownership_boundary_clarity: +2, upfront_clarity_need: +2

**Q2 After initial alignment, what source of truth helps you most?**

A. A written summary of decisions, responsibilities, and next steps — I need to be able to return to what we agreed without relying on memory or asking again.

B. A shared board or plan with priorities, owners, and progress — I want a live view of what is happening, not just a snapshot from the last meeting.

C. Clear task ownership in the work-tracking system — I work best when responsibility is attached directly to the task, not discussed separately.

D. I usually rely more on conversation than written tracking — I find that regular dialogue keeps me aligned more reliably than maintaining documents.

Hidden mapping:
- A: clarity_via_written_context: +2, verbal_alignment_preference: -1
- B: visibility_preference: +2, clarity_via_written_context: +1
- C: ownership_boundary_clarity: +2, clarity_via_written_context: +1
- D: verbal_alignment_preference: +2, clarity_via_written_context: -1

**Q3 When you join an existing team or project, what helps you integrate fastest?**

A. Documentation, previous decisions, and project context — I want to understand what was tried, decided, and why before I start forming my own view.

B. A conversation with someone who knows the history — written context only goes so far. I learn fastest by talking to someone who can answer what documents cannot.

C. Looking at real examples: tasks, code, tickets, or past work — the actual work tells me more about how a team operates than any description of it.

D. Understanding team norms, roles, and expectations — I integrate faster when I know how the team works together and what is expected of me, not just what the work is.

Hidden mapping:
- A → clarity_via_written_context: +2, upfront_clarity_need: +1
- B → verbal_alignment_preference: +2, clarity_via_written_context: -1
- C → learning_by_doing: +2, clarity_via_artifact_context: +2, ambiguity_tolerance: +1
- D → social_context_need: +2, ownership_boundary_clarity: +2

**Q4 When working with someone new, which situation would make you most cautious?**

A. No one is clear who owns the decision or next step — unclear ownership creates duplication, dropped work, and confusion about who to go to when something needs resolving.

B. Plans change verbally, but the change is not visible anywhere — changes that never land in the system are easy to forget and hard to hold anyone to.

C. People act on assumptions without checking understanding — decisions are made on things nobody actually agreed on.

D. Risks or mistakes are not raised early — I find it harder to trust a collaboration where problems are held back until they become urgent.

Hidden mapping:
- A: ownership_boundary_clarity: +2
- B: clarity_via_written_context: +2, visibility_preference: +1
- C: explicit_alignment_need: +2
- D: risk_transparency_need: +2

## Section 2: Staying aligned during work / Collaboration rhythm

Questions about ongoing collaboration:

**Q1 During shared work, when do you prefer alignment conversations about goals, responsibilities, progress, and changes to happen?**

A. At scheduled check-ins, so people can prepare and avoid unnecessary interruptions — I find that alignment works better when people can come ready, not pulled in mid-thought.

B. Spontaneously, whenever alignment needs attention — I would rather address a misalignment the moment I notice it than wait for a scheduled moment that may not come soon enough.

C. Scheduled only when there is a clear reason, such as a blocker, decision, or major change — conversations without a purpose tend to create more noise than clarity.

D. At a regular rhythm, with spontaneous conversations for urgent issues — I want a predictable baseline with room to escalate when something cannot wait.

Hidden mapping:
- A: scheduled_communication_preference: +2, focus_protection: +1
- B: immediate_alignment_preference: +2, verbal_alignment_preference: +1
- C: focus_protection: +2, scheduled_communication_preference: +1
- D: scheduled_communication_preference: +1, immediate_alignment_preference: +1, verbal_alignment_preference: +1

**Q2 During shared work, where should important updates, decisions, and progress be captured?**

A. In a written summary or decision note — I need decisions separated from the noise of the thread so I can find and reference them later.

B. On a shared board or plan with owners and progress — I want the state of the work visible in one place, not scattered across messages.

C. In task comments, tickets, or pull requests close to the work — context belongs next to what it describes, not in a separate document that may drift.

D. In conversation first; written capture is only needed when something is unclear, risky, or important — I do not think everything needs to be documented, only the things worth coming back to.

Hidden mapping:
- A: clarity_via_written_context: +2, verbal_alignment_preference: -1
- B: visibility_preference: +2, clarity_via_written_context: +1
- C: clarity_via_artifact_context: +2, clarity_via_written_context: +1
- D: verbal_alignment_preference: +2, clarity_via_written_context: -1

**Q3 During shared work, how much visibility do you prefer into collaborators' progress before the work is finished?**

A. Frequent small updates while work is still in progress — whether through a board, messages, or brief updates, I find it easier to coordinate and help when I have a live sense of where things are, not just the final result.

B. Updates when progress, blockers, or assumptions meaningfully change — I do not need a running feed, just a signal when something shifts that I should know about.

C. Updates at agreed checkpoints, such as review points or milestones — predictable moments work better for me than continuous updates that interrupt flow.

D. Minimal updates unless help, coordination, or a decision is needed — I trust people to surface what matters and prefer not to monitor what does not require my attention.

Hidden mapping:
- A: visibility_preference: +2, immediate_alignment_preference: +1
- B: visibility_preference: +1, risk_transparency_need: +1
- C: focus_protection: +2, scheduled_communication_preference: +1
- D: ambiguity_tolerance: +2, focus_protection: +1

This makes it clearer that it is about amount/frequency of visibility, not tool.
A nice user instruction could be: "Answer based on how much visibility you prefer, not where the update happens."
So Section 2 now has clean separation:
1.	When do we talk? 
2.	Where do we capture important information? 
3.	How much visibility do we want while work is in progress?

## Section 3: Planning and delivery style

**Q1 Which way of organizing work feels most natural to you?**

A. Fixed cycles with planning, review, and reflection — I work best when there is a predictable rhythm with clear moments to plan, deliver, and improve.

B. Continuous flow through a visible board or backlog — I prefer to pull from a prioritized list and keep work moving without waiting for the next cycle to start. The backlog doesn't need to be complete before I begin.

C. A hybrid approach with some planning rhythm and flexible flow — I want the predictability of regular planning without the rigidity of fixed cycles that cannot respond to change.

D. Larger upfront planning with milestones and dependencies clarified early — I find it easier to execute when the full scope, sequence, and dependencies are visible from the start.

Hidden mapping:
- A: iteration_preference: +2, scheduled_communication_preference: +1
- B: flow_preference: +2, ambiguity_tolerance: +1
- C: iteration_preference: +1, flow_preference: +1
- D: upfront_planning_preference: +2, upfront_clarity_need: +1

This is probably the core question for this section.
Rough hidden mapping:
•	A = Scrum-like 
•	B = Kanban-like 
•	C = Scrumban / hybrid 
•	D = plan-driven / waterfall-friendly 
But users should not see those labels.

**Q2 Before starting a larger piece of work, what helps you plan most effectively?**

A. Clarifying scope, milestones, dependencies, and risks upfront — I need the full picture before I start so I can move confidently without discovering surprises mid-execution.

B. Creating a prioritized backlog with enough detail for the next step — I plan just enough to start well and let the rest emerge as I learn more.

C. Starting with a rough direction and refining the plan as we learn — I find that too much planning before we understand the problem creates false certainty.

D. When uncertainty or risk is high, running a small experiment first and planning based on what we learn — I would rather build the plan on something real than commit to assumptions we have not tested.

Hidden mapping:
- A: upfront_planning_preference: +2, upfront_clarity_need: +1
- B: flow_preference: +2, ambiguity_tolerance: +1
- C: adaptive_planning_preference: +2, ambiguity_tolerance: +2, upfront_planning_preference: -1
- D: evidence_based_planning_preference: +2, risk_transparency_need: +1, ambiguity_tolerance: +1

Rough hidden mapping explanation:
•	A = upfront planning from known constraints 
•	B = backlog / next-step planning 
•	C = adaptive planning while work progresses 
•	D = evidence-informed planning when uncertainty is high

**Q3 When priorities change during shared work, what makes the change easiest for you to work with?**

A. The responsible person updates the backlog, board, or plan clearly — a priority change that only lives in conversation has not really been made yet.

B. The reason, trade-offs, and risks are explained before the change is made — I can adapt more easily when I understand what was considered and what we are accepting.

C. The change is discussed with the people affected before it is finalized — I need to be part of the conversation before the change lands, not just informed after.

D. Changes are grouped into planning points rather than introduced continuously — I find it harder to work well when priorities can shift at any moment without a boundary.

Hidden mapping:
- A: clarity_via_written_context: +2, visibility_preference: +1, verbal_alignment_preference: -1
- B: risk_transparency_need: +2, upfront_clarity_need: +1
- C: participatory_decision_preference: +2, explicit_alignment_need: +1
- D: planning_boundary_protection: +2, focus_protection: +1, scheduled_communication_preference: +1

This is not asking:
"Do you accept priority changes?"
It is asking:
"What makes priority changes feel understandable, fair, and workable?"
Hidden mapping:
•	A = visible source of truth to reflect the change, not invisible verbal changes
•	B = reasoning / trade-off clarity 
•	C = participatory change management 
•	D = stability / focus protection 
More detailed internal mapping:
A → source_of_truth_preference, visibility_preference, planning_ownership_clarity
B → reasoning_preference, tradeoff_visibility, risk_awareness
C → participatory_decision_preference, impact_discussion_preference, collaboration_preference
D → stability_preference, focus_protection, change_control_preference
This question is good for detecting friction around priority changes.
For example:
•	Person A chooses A: "Please make the change visible in the system." This person needs the source of truth to reflect the change. For example: "If the workshop scope changed, please update the ticket, board, planner, or written plan so it is visible." This is about avoiding invisible verbal changes.
•	Person B chooses C: "Please discuss the change with affected people first." This person needs participation before impact. For example: "Before changing my scope or deadline, talk to me or the team affected by it." This is not just about explanation. It is about involvement.
•	Person C chooses D: "Please do not keep changing priorities mid-flow." This person needs stability and focus. For example: "Please do not keep changing priorities every day. Let's collect changes and handle them during sprint planning, weekly planning, or another agreed planning point unless it is urgent." This is about reducing disruption.
•	Person X chooses B, This person needs to understand the why behind the change: "Why are we dropping this task?", "What risk are we accepting?", "What are we not doing because of this new priority?" This is about reasoning and transparency.

**Q4 When a team has already committed to a plan, how should new requests be handled?**

A. Add them only if the team explicitly removes or deprioritizes existing work — if we are taking on something new, we need to be honest about what we are no longer doing.

B. Decide case by case after discussing urgency, impact, and risks — I am open to changing the plan, but I want the decision to be conscious, not reactive.

C. Add them flexibly if they are clearly valuable — I think the ability to respond to something important should not be blocked by process.

D. Save them for the next planning point unless they are truly urgent — I need the plan to mean something, and that requires protecting it from constant interruption.

Hidden mapping:
- A: capacity_protection: +2, sustainable_pace_preference: +1
- B: tradeoff_visibility: +2, risk_transparency_need: +1, participatory_decision_preference: +1, change_tolerance: +1
- C: change_tolerance: +2, ambiguity_tolerance: +1, capacity_protection: -1, sustainable_pace_preference: -1
- D: planning_boundary_protection: +2, capacity_protection: +1, sustainable_pace_preference: +1

This version is cleaner:
•	A = capacity trade-off required 
•	B = case-by-case trade-off discussion 
•	C = flexible responsiveness 
•	D = planning boundary / focus protection
What each answer means
A. Add them only if something else is removed or deprioritized
This person thinks capacity should be respected.
They are not necessarily against change, but they want the team to be honest:
"If we add something new, what are we dropping?"
Hidden mapping:
A = capacity awareness / scope trade-off / focus protection
This is very healthy in teams where people often overload themselves.
________________________________________
B. Discuss urgency, impact, and trade-offs before changing the plan
This person is open to changing the plan, but wants a conscious decision.
They want questions like:
"How urgent is this?"
"Who will be affected?"
"What risk does this create?"
"What happens if we do it later?"
Hidden mapping:
B = trade-off visibility / collaborative replanning / risk awareness
This is probably the most "balanced" answer, so be careful: many people may choose it.
________________________________________
C. Add them flexibly if they are clearly valuable
This person values responsiveness.
They may think:
"If something important appears, the process should not slow us down too much."
Hidden mapping:
C = flexibility / opportunity responsiveness / high change tolerance
This can be very useful in fast-moving teams, early-stage products, incidents, discovery work, or client-driven environments.
Possible friction: others may experience this as chaotic if it happens too often.
________________________________________
D. Save them for the next planning point unless they are truly urgent
This person values planning boundaries and protected focus.
They may think:
"If everything can interrupt the plan, then the plan does not mean much."
Hidden mapping:
D = planning boundary protection / stability / sprint protection
This is especially relevant for sprint-based teams or teams doing deep work.

**Q5 If priorities or plans are being managed by someone else, what do you rely on them for most?**

A. Keeping the backlog, board, or plan up to date — I need the system to reflect what is actually true, not what was decided two weeks ago.

B. Making trade-offs, risks, and alternative paths visible — I want to understand not just what was decided but what was considered and what we are giving up.

C. Protecting focus by limiting unplanned changes — I rely on whoever holds the plan to push back on requests that would disrupt work already in progress.

D. Involving the team before changing scope, timing, or responsibilities — I expect that changes affecting my work or ownership will include me before they are finalized.

What this question measures
This question is especially useful for manager–teammate, product–engineering, and lead–team relationships.
It asks:
"What does responsible planning leadership look like to you?"
Hidden dimensions:
planning_ownership_clarity
source_of_truth_preference
tradeoff_visibility
risk_awareness
focus_protection
participatory_decision_preference
responsibility_change_sensitivity
What each answer means
A. Keep the backlog, board, or plan up to date
This person expects operational clarity.
They may think:
"If priorities or responsibilities change, the system of record should show it."
Hidden mapping:
A = source of truth / operational visibility / planning maintenance
This is important for avoiding verbal-only changes and later confusion.
________________________________________
B. Make trade-offs, risks, and alternative paths visible
This person wants to understand the reasoning behind planning decisions.
They may think:
"Don't just tell us what changed. Show what options were considered, what we are giving up, and what risk we are accepting."
Hidden mapping:
B = strategic transparency / trade-off clarity / risk awareness
This is good for complex work where decisions have consequences.
________________________________________
C. Protect focus by limiting unplanned changes
This person expects the planner or priority-owner to guard the team's attention and capacity.
They may think:
"Please do not let every new request become an interruption."
Hidden mapping:
C = focus protection / stability / interruption sensitivity
This is especially relevant for teams with too much context switching.
________________________________________
D. Involve the team before changing scope, timing, or responsibilities
This person wants affected people to be included before changes are finalized.
They may think:
"If the change affects my work, timeline, or ownership, I should be part of the discussion."
Hidden mapping:
D = participatory planning / impact discussion / responsibility-change sensitivity
This is important for trust and fairness.

Hidden mapping:
- A: clarity_via_written_context: +2, visibility_preference: +2
- B: tradeoff_visibility: +2, risk_transparency_need: +2
- C: focus_protection: +2, planning_boundary_protection: +2
- D: participatory_decision_preference: +2, ownership_boundary_clarity: +1

**Q6 When a team needs to improve how it works, what approach feels most useful to you?**

A. Regular retrospectives or reflection sessions — I think improvement needs dedicated time to happen. Without a structured moment, it tends to get crowded out by delivery.

B. Small improvements whenever a problem appears — I prefer to fix things in the moment rather than accumulate problems until the next formal session.

C. Looking at evidence such as delivery patterns, quality issues, incidents, or feedback — I trust observable signals more than opinions when deciding where to improve.

D. Informal conversations when something feels off — I find that honest conversations between the right people often resolve things faster than any formal process.

What this question measures
This question asks:
"How should a team notice problems and improve its process?"
It is not only about Scrum retrospectives. It is about someone's preferred improvement style.
Hidden dimensions:
reflection_style
structured_reflection_preference
continuous_improvement
evidence_based_improvement
informal_feedback_preference
process_improvement_orientation
What each answer means
A. Regular retrospectives or reflection sessions
This person likes a dedicated space for the team to pause and reflect.
They may think:
"If we do not create time to reflect, important patterns may never be discussed."
Hidden mapping:
A = structured reflection / regular team learning / process awareness
This is good for teams that need intentional improvement habits.
________________________________________
B. Small improvements whenever a problem appears
This person prefers fixing issues in the moment instead of waiting for a formal session.
They may think:
"If we see something not working, let's improve it now."
Hidden mapping:
B = continuous small improvement / action orientation / low ceremony
This can work well in teams that communicate openly and do not need much formal process.
________________________________________
C. Evidence from delivery, quality, incidents, or feedback
This person wants improvement to be grounded in observable signals.
They may think:
"Let's not improve based only on opinions. Let's look at delivery data, bugs, incidents, user feedback, or repeated blockers."
Hidden mapping:
C = evidence-based improvement / systems thinking / quality orientation
This is useful when teams need to distinguish feelings from patterns.
________________________________________
D. Informal conversations when something feels off
This person prefers lightweight human sensing over formal process.
They may think:
"We should talk when we notice tension, confusion, or something not feeling right."
Hidden mapping:
D = informal reflection / relational sensing / low-structure feedback
This can work well in high-trust teams, but may miss deeper patterns if people avoid difficult conversations.

**Q7 When a team reflects on how work is going, what topics should be included?**

A. Work process, planning, blockers, and delivery flow — I think the most important thing to examine is whether the team can execute and deliver reliably.

B. Technical quality, incidents, risks, and maintainability — I think reflection should include whether the work we are producing is actually sound, not just delivered on time.

C. Collaboration patterns, communication, and decision-making — these topics feel less urgent than delivery, but left unexamined they tend to create friction that affects people and eventually shows up in the work.

D. Team safety, pressure, conflict, and how people are treated — when people do not feel safe being honest, real problems stay hidden until they are already affecting the work. That is usually when they are hardest to fix.

Hidden mapping
A → process_reflection_preference, delivery_flow_orientation
B → technical_reflection_preference, quality_orientation, risk_awareness
C → collaboration_reflection_preference, communication_awareness, decision_process_awareness
D → psychological_safety_preference, conflict_awareness, interpersonal_safety_orientation
Possible internal dimensions for section 3
You could define Section 3 dimensions like this (should be corrected/updated)
delivery_rhythm_preference
planning_depth_preference
flow_preference
iteration_preference
upfront_planning_preference
adaptive_planning
experimentation_preference
source_of_truth_preference
priority_change_preference
focus_protection
change_tolerance
tradeoff_visibility
participatory_decision_preference
reflection_preference
evidence_based_improvement
________________________________________
Example scoring shape
For each answer, you can assign weights like this:
{
  "question_id": "delivery_rhythm_01",
  "answer_id": "A",
  "weights": {
    "iteration_preference": 2,
    "structured_process_preference": 2,
    "reflection_preference": 1,
    "flow_preference": -1
  }
}

## Section 4: Quality, risk, and delivery expectations

**Q1 When work is under time pressure, what most helps you feel comfortable calling it done or handing it over?**

A. It solves the core problem reliably. Risks, reviews, and follow-up plans matter, but they should not always be the gate.

B. I have a clear picture of what is still open and can make it visible — knowing and naming the gaps is what makes handing over feel responsible to me.

C. Someone else has reviewed it. I trust the handoff more when another person has checked the work or challenged my assumptions.

D. What is not done now has a clear owner and plan. Iteration is fine, but only with accountability.

**Q2 You discover a problem with work that has already been handed off or shared. What is your instinct?**

A. I tell the relevant people early, even before I know everything. Surprises are worse than uncertainty.

B. I assess how serious it is first. A proportionate response serves everyone better than immediate alarm.

C. I try to fix it quickly before involving others. If I can resolve it responsibly, I do not want to create unnecessary noise.

D. I look for why it happened, not only how to fix it. Preventing the same problem matters as much as resolving this one.

**Q3 Your team needs to reduce scope or quality to meet a deadline. What is your instinct?**

A. I make the compromise visible and documented, but I do not necessarily seek agreement. Transparency is enough for me.

B. I protect the quality bar, even if it means pushing back on the timeline or scope reduction. Some risks are not worth accepting.

C. I accept the compromise and trust we will improve later. Progress over perfection.

D. I need explicit agreement from the people affected before moving forward. Alignment is not optional.

**Q4 You are starting a significant piece of work, and the expected quality bar has not been clearly defined. What is your instinct?**

A. I clarify what "good enough" means before going too far. Shared expectations prevent rework.

B. I define my own quality bar and make my assumptions visible. Someone needs to create a starting point.

C. I start with a smaller version and use feedback to calibrate the quality bar. Learning through progress is safest.

D. I use existing team standards or past examples as the quality bar. Consistency matters when expectations are unclear.

It distinguishes:
•	A = clarify before execution 
•	B = self-directed assumption-making 
•	C = iterative calibration 
•	D = precedent / standards-based quality

**Q5 Someone finds a problem in work you considered done. What is your instinct?**

A. I acknowledge it and focus on fixing it. If there is a real problem, ownership matters more than explanation.

B. I first assess how much it matters. The right response depends on actual impact, not the finding itself.

C. I want to understand why it is considered a problem. Not every concern reflects a real gap.

D. I want to revisit expectations and assumptions. If the quality bar was not shared clearly, the finding itself is worth examining.

Hidden mapping:
•	A = ownership / repair-first 
•	B = impact assessment / proportional response 
•	C = validation before acceptance / healthy skepticism 
•	D = expectation alignment / shared definition of done
This is the next big layer. It should focus on trust, feedback, conflict, and support.

## Section 5: Feedback and psychological safety

Core question:
What makes feedback, disagreement, and uncertainty feel safe enough to work with?
Suggested questions:

**Q1 Someone tells you that your feedback was too harsh, but you believe the concern you raised was valid. What is your instinct?**

A. I focus on whether my feedback was accurate; useful feedback should not be dismissed because it was uncomfortable.

B. I adjust my delivery because feedback only works if the other person can actually use it.

C. I want to separate the two issues: whether the concern was valid, and whether the delivery made it harder to hear.

D. I ask what specifically felt harsh so we can agree on a better way to discuss concerns next time.

**Q2 You receive feedback that feels unnecessarily harsh, but the concern may be valid. What is your instinct?**

A. I try to separate the message from the delivery. If there is something useful in the feedback, I want to understand it even if the style was poor.

B. I ask for the feedback to be given in a way I can actually use. Directness is okay, but the delivery needs to support the conversation, not make it harder.

C. I want to discuss both the concern and how it was delivered. The feedback itself may matter, but so does the communication pattern around it.

D. I question the feedback more if the delivery feels unclear, exaggerated, or unfair. If the style distorts the message, I need to examine whether the concern is valid.

**Q3 When you have a concern about someone's work or behavior, what is your default instinct?**

A. I raise it privately first. Public discussion should be a last resort.

B. I raise it where the relevant people can see or hear it. Transparency helps the team understand and respond to problems together.

C. I adjust based on the person and relationship. Some people can handle public discussion; others need more care or privacy.

D. I raise it in the context where the concern appeared. If it happened in a meeting, task, review, or shared discussion, I usually address it there.

**Q4 Someone raises a concern about your work or behavior in front of others. What matters most to you?**

A. That the concern is relevant to the group. If others are affected or need the context, public discussion can be appropriate.

B. That it is handled respectfully and without embarrassment. Public feedback can be okay, but not if it undermines trust or dignity.

C. That I have a chance to respond or clarify context. If a concern is raised publicly, I need room for my perspective too.

D. That the person follows up privately afterward. Public discussion may solve the immediate issue, but repair often needs a separate conversation.

## Section 6: Autonomy and support

Core question:
What balance of independence, involvement, and support feels healthy?

**Q1 What starts feeling like micromanagement to you?**

A. Frequent status requests without a clear reason. I do not mind sharing progress, but repeated check-ins feel frustrating when they do not change anything or help the work.

B. Detailed direction on work I already own. Once I am responsible for something, too much instruction about how to do it can feel like my judgment is not trusted.

C. Decisions being reopened repeatedly after alignment. If we already agreed on direction, repeated second-guessing makes ownership feel unstable.

D. Oversight that feels more like control than support. I can accept involvement, but not when it seems driven by anxiety, distrust, or a need to approve every step.

**Q2 What starts feeling like abandonment to you?**

A. Expectations change without discussion or support. I can handle change, but not when I am left to absorb it alone or guess what is now expected.

B. Important decisions become hard to get help on. I can work independently, but blocked decisions or unavailable guidance make me feel unsupported.

C. I only hear feedback when something goes wrong. If there is no encouragement, calibration, or signal until a problem appears, support starts to feel absent.

D. Coordination disappears even though the work is interdependent. Autonomy is fine, but connected work still needs communication, timing, and shared awareness.

**Q3 When you are unsure how to move forward, what kind of support feels most helpful?**

A. Help clarifying the goal, constraints, and what matters most. I can move forward once the direction is clearer.

B. A concrete decision or recommendation from someone with context. I do not want to stay blocked when someone else can unblock the path.

C. A conversation to think through options together. I value support that helps me reason, not just gives me an answer.

D. Space to explore first, then bring back what I learn. I prefer not to involve others before I have formed my own view.

**Q4 What makes ownership of work feel healthy to you?**

A. I know what decisions are mine to make. Clear boundaries make ownership feel safe.

B. I can ask for help without it being seen as a lack of competence. Support should not reduce trust.

C. Others stay informed enough that my work does not become isolated. Ownership should not mean disappearing.

D. I have enough freedom to make trade-offs without needing approval for every step. Ownership should come with real authority.

## Section 7: Meetings and live collaboration

Core question:
When is real-time interaction useful, and when does it become noise?

**Q1 — What kind of a scheduled status meeting feels most useful?**

A. Short, structured, and focused on blockers — I want to leave knowing what is moving and what is not, not having discussed everything.

B. Discussion-based, with space for context — sometimes the status itself is less useful than understanding why things are the way they are.

C. Mostly async updates, with meetings only when something actually needs live discussion — the default should be written, not scheduled.

D. A regular rhythm, even if there is not always much to discuss — the consistency itself has value, and I would rather have a quiet meeting than lose the habit.

**Q2 — What is the strongest justification for interrupting someone with an unscheduled conversation?**

A. Something is genuinely blocked and every hour of waiting has a real cost. I would rather interrupt than let the work stall or the decision stay open longer than it needs to.

B. The topic is too sensitive or too easily misread in writing. Some things need tone, expression, and real-time response — not a message that sits there being interpreted.

C. The written thread has already taken longer than a short conversation would. When async stops being efficient, switching to real-time is not an interruption — it is the right call.

D. Honestly, almost nothing justifies it by default. Most things can wait, and I think respecting someone's focus means letting them choose when they are available rather than pulling them out of it.

**Q3 — If a recurring meeting could only guarantee one of these things, which would make it worth keeping?**

A. A clear agenda before it starts and a visible outcome, decision, or next step after. Without those two things I struggle to justify the time, no matter how often we meet.

B. The team actually being in the same space regularly. Some of what keeps collaboration working is not output — it is familiarity, rhythm, and the kind of connection that does not happen in tasks and tickets.

C. Shared awareness of what is shifting, what is blocked, and what people need from each other. The cost of misalignment is usually higher than the cost of the meeting itself.

D. The genuine freedom to cancel it when it is not needed. A meeting that earns its place every time it happens feels very different from one that just recurs because it was scheduled.

**Q4 — What makes a meeting feel draining or unnecessary?**

A. No clear decision, outcome, or next step came from it — I can accept that a meeting was hard or long, but I struggle when I cannot point to what it produced.

B. The same context could have been shared in writing and people could have responded in their own time — synchronous time is expensive and I notice when it is not actually needed.

C. Too many people were included without a clear reason to be there — it signals either unclear ownership or a habit of over-involving people as a substitute for actual communication.

D. It pulled me out of focused work without delivering enough value to justify the cost — the interruption itself is the problem, not just the meeting content.

## Section 8: Pressure, urgency, and motivation

**Q1 — When work becomes urgent, what kind of communication helps you most?**

A. A calm, clear statement of what matters most right now and what can wait — I do not need the urgency performed, I need the priority to be unambiguous so I can move without second-guessing.

B. Enough context to understand why it is urgent and what is actually at stake — urgency without reasoning makes it hard for me to make good decisions about how to respond.

C. A quick collaborative moment to agree on what gets dropped or deferred — I can move fast once we are honest about what we are not doing anymore.

D. A written summary I can come back to — when things are urgent, live conversation creates noise I have to carry in my head. Written expectations let me focus instead of remember.

**Q2 — Your manager addresses the team before a critical deadline. Which would actually motivate you most?**

A. "If we don't deliver this, heads will roll."

B. "This is not university. We have to deliver."

C. "I think this is a genuinely exciting milestone. Let me know if anything is blocking you."

D. Nothing — the deadline is visible, the priorities are clear. A speech would mostly get in the way.

**Q3 — When a manager or colleague communicates urgency in a way that bothers you, what do you usually do?**

A. I raise it directly with the person — not necessarily in the moment, but soon after. I think patterns like that are worth naming even when it is uncomfortable.

B. I absorb it and focus on the work — I let the communication style go and do not raise it unless it becomes a repeated pattern that is genuinely affecting the team.

C. I mention it to someone I trust, but not to the person directly — sometimes naming it out loud is enough, even without confrontation.

D. It depends on the relationship and how often it happens — a one-off I can usually let go, but if it keeps happening I will say something.

**Q4 — During a high-pressure or urgent situation, someone tells you that your tone, word choice, or communication style felt too intense, too blunt, or hard to receive. What is your instinct?**

A. I take it seriously and adjust — if my delivery made it harder for someone to think clearly or act well, that is my problem to fix regardless of the pressure we were both under.

B. I want to understand what specifically landed badly before I change anything — even under pressure, adjusting without understanding what went wrong usually does not help either of us.

C. I acknowledge the reaction but do not automatically see it as something to fix — sometimes intensity reflects the situation honestly, and I think that can be said without apologizing for it.

D. I notice whether it is a pattern or a one-off — pressure situations are not normal conditions, and a single reaction does not always mean I need to change how I communicate generally.

## Section 9: Conflict and tension handling

**Q1 — You notice tension between yourself and a colleague. What feels most important to you first?**

A. Understand what the disagreement actually is — the feeling of tension is not always a reliable signal of what is really wrong, and I want to know what I am actually dealing with before responding to it.

B. Make sure it does not affect the wider team — tension between two people has a way of changing the atmosphere for everyone, and I feel responsible for containing that boundary early.

C. Give both of us some space before trying to address it — when people are still in the feeling of tension, conversations tend to produce reactions rather than resolution.

D. Name it directly before it has time to harden — the longer something sits unacknowledged between two people, the more weight it accumulates.

**Q2 — When should an issue be escalated?**

A. As soon as delivery or coordination is genuinely at risk — waiting for the right process when something real is breaking feels like prioritising comfort over responsibility.

B. When the issue involves a decision or authority that neither person actually has — some conflicts cannot be resolved directly because the people involved do not have the power to resolve them, and escalation in that case is not avoidance, it is the right path.

C. When the same pattern repeats without improvement — a single incident rarely justifies escalation, but a pattern that nobody is addressing is a different problem entirely.

D. Only when the people involved have genuinely tried and cannot resolve it responsibly on their own — escalation should be a last resort, not a shortcut around a difficult conversation.

**Q3 — When a difficult conversation is necessary, what do you most want it to produce?**

A. A clear decision or resolution — I can handle hard conversations well when they end with something concrete. Ambiguity after a difficult exchange is harder for me than the conversation itself.

B. Mutual understanding of how each person sees it — I do not always need agreement, but I need to feel that both perspectives were genuinely heard before we move on.

C. The relationship staying intact and workable — the outcome matters, but not more than the two people being able to collaborate afterwards without it hanging over everything.

D. Both people owning something — I find conversations more honest when neither person is only delivering or only receiving. Shared accountability changes the dynamic entirely.

**Q4 — When tension or conflict between you and a colleague is not resolving itself, would you want someone to help facilitate, and who should that be?**

A. Yes — a neutral peer who knows both of us and has enough context to help without taking sides.

B. Yes — a manager or someone with enough authority to move things forward if the conversation stalls.

C. Only if both people agree to it — facilitation imposed on one side usually makes things worse, not better.

D. No — I think bringing in a third party changes the dynamic in ways that are hard to undo. I would rather resolve it directly even if it takes longer.

**Q5 — When conflict or tension appears in a team, what do you believe about how it resolves?**

A. It needs active intervention — left alone, tension rarely improves and usually hardens into something more difficult to address later.

B. It depends on the people involved — not everyone processes conflict the same way, and assuming everyone needs the same kind of conversation, space, or ritual usually creates its own friction.

C. It often resolves itself if people are given enough space and time — premature intervention can make people feel managed rather than trusted.

D. It resolves more reliably when the team has regular practices that make tension safe to surface early — not just when something breaks. Without those habits, people wait too long or avoid it entirely.

**Q6 — You are in a leading role, and a team member comes to you and tells you they are in conflict with a colleague. What is your instinct?**

A. I listen first and ask what they have already tried — I want to understand the situation before doing anything, and I want to support them in resolving it directly if that is still possible.

B. I offer to facilitate a conversation or organize a moderation between both people — bringing them together in a structured way feels more useful than handling each side separately.

C. I talk to the other person separately to hear their perspective before deciding anything — one account is never the whole picture.

D. I assess whether this is affecting the wider team before deciding how to respond — a conflict between two people is sometimes a symptom of something bigger, and I want to understand that before focusing only on the two people involved.

## Section 10: Energy, satisfaction, and drain

Core question:
What kind of work gives this person energy, meaning, or exhaustion?

**Q1 — What motivates you most in your work?**

A. The craft itself — I find genuine satisfaction in building things well, solving hard problems, and improving how things work. The work is its own reward.

B. Understanding the purpose — I need to know why something matters. Motivation drops significantly when I cannot connect the work to a meaningful outcome.

C. Ownership and trust — being given real responsibility and the freedom to make decisions is what makes work feel worth doing.

D. The people and relationships — the team, the collaboration, the shared experience of building something together matters more to me than the work itself.

**Q2 — What do you want more of in your work?**

A. Deep focus time — uninterrupted stretches where I can actually think, not just react to whatever is most urgent.

B. Clearer priorities — not more information, just a more honest signal about what actually matters most right now.

C. Better collaboration and feedback — work that involves real exchange, not just parallel effort with occasional updates.

D. More ownership and decision-making space — I want to be trusted with the whole problem, not just the execution of someone else's solution.

**Q3 — What kind of recognition feels most meaningful?**

A. Someone notices the quality and care in my work — not just that it is done, but that the way it was done actually mattered to someone.

B. Someone trusts me with more ownership or a harder problem — recognition through responsibility feels more real to me than words.

C. Someone acknowledges the effort during a difficult or pressured period — being seen when things are hard means more than being praised when things are easy.

D. Someone connects my work to a bigger outcome — I find it meaningful when I can see how what I did actually moved something that mattered.

**Q4 — What drains you most at work?**

A. Avoided conflict or indirect communication — when problems are visible but nobody names them, the silence itself becomes exhausting to work inside.

B. Redundant process, unnecessary meetings, or overhead that does not produce anything — I can accept structure when it serves something, but process for its own sake steadily drains my motivation.

C. Work that feels disconnected from any meaningful outcome — I can push through hard or boring work when I understand why it matters. When I cannot see that, the effort starts to feel hollow.

D. Feeling underutilized or invisible — when my judgment, skills, or contribution are consistently overlooked or not sought out, it is quietly exhausting in a way that is hard to name but hard to ignore.

**Q5 — At the end of a good workday, what most often makes it feel like time well spent?**

A. I made visible progress on something that actually matters — not busy work, not preparation for future work, but real movement on something with weight.

B. I helped someone or removed something that was blocking them — knowing the day made someone else's work easier or better is enough for me.

C. I understood something I did not understand before — a day where my mental model of the problem, the system, or the people improved feels well spent even without visible output.

D. I left things better than I found them — a cleaner codebase, a clearer process, a resolved tension. Incremental improvement in the right direction is quietly satisfying in a way that big visible progress sometimes is not.
