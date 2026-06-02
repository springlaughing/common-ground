using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using Microsoft.EntityFrameworkCore.Migrations;

namespace CommonGround.Api.Persistence.Migrations;

internal static class SeedDataHelper
{
    internal static Guid G(string seed)
    {
        var hash = SHA256.HashData(Encoding.UTF8.GetBytes(seed));
        return new Guid(hash[..16]);
    }

    // ── IDs ────────────────────────────────────────────────────────────────

    internal static readonly Guid QvId = G("qv.v1.0");

    // Question IDs (s{section}q{question})
    internal static Guid Q(int s, int q) => G($"q.s{s}q{q}");

    // AnswerOption IDs (ao.s{s}q{q}.{letter})
    internal static Guid AO(int s, int q, char letter) => G($"ao.s{s}q{q}.{letter}");

    // DimensionWeight IDs
    internal static Guid DW(int s, int q, char letter, string dim) => G($"dw.s{s}q{q}.{letter}.{dim}");

    // InsightSnippet IDs
    internal static Guid IS(string dim) => G($"is.{dim}");

    // DimensionGroup IDs
    internal static Guid DG(string groupId) => G($"dg.{groupId}");

    // DimensionGroupMembership IDs
    internal static Guid DGM(string groupId, string dim) => G($"dgm.{groupId}.{dim}");

    // DimensionMaxScore IDs
    internal static Guid DMS(string dim) => G($"dms.{dim}");

    // ── Seed entry point ───────────────────────────────────────────────────

    internal static void SeedUp(MigrationBuilder mb)
    {
        InsertQuestionnaireVersion(mb);
        var questions = BuildQuestions();
        InsertQuestionsAndOptions(mb, questions);
        InsertDimensionWeights(mb, questions);
        InsertDimensionMaxScores(mb, questions);
        InsertInsightSnippets(mb);
        InsertDimensionGroups(mb);
        InsertDimensionGroupMemberships(mb);
    }

    internal static void SeedDown(MigrationBuilder mb)
    {
        mb.Sql("DELETE FROM \"DimensionGroupMemberships\"");
        mb.Sql("DELETE FROM \"DimensionGroups\"");
        mb.Sql("DELETE FROM \"InsightSnippets\"");
        mb.Sql("DELETE FROM \"DimensionMaxScores\"");
        mb.Sql("DELETE FROM \"DimensionWeights\"");
        mb.Sql("DELETE FROM \"AnswerOptions\"");
        mb.Sql("DELETE FROM \"Questions\"");
        mb.Sql("DELETE FROM \"QuestionnaireVersions\"");
    }

    // ── QuestionnaireVersion ───────────────────────────────────────────────

    private static void InsertQuestionnaireVersion(MigrationBuilder mb) =>
        mb.Sql($"""
            INSERT INTO "QuestionnaireVersions" ("Id","VersionNumber","IsActive","CreatedAt")
            VALUES ('{QvId}','1.0',true,'2026-06-02T00:00:00+00:00')
            """);

    // ── Questions ─────────────────────────────────────────────────────────

    private sealed record WeightDef(string DimId, int Weight);
    private sealed record OptionDef(char Letter, string Text, WeightDef[] Weights);
    private sealed record QuestionDef(int Section, int LocalQ, int OrderIndex, string Text, OptionDef[] Options);

    private static QuestionDef[] BuildQuestions() =>
    [
        // ── Section 1 ───────────────────────────────────────────────────
        new(1,1,1,"When starting work with someone new, what helps you feel ready to collaborate?",
        [
            new('A',"Written goals, context, and expectations — I need to understand what we are working toward before I can contribute. A document I can return to is more reliable than a conversation I have to remember.",
                [new("upfront_clarity_need",2),new("clarity_via_written_context",2),new("verbal_alignment_preference",-1)]),
            new('B',"A live conversation to ask questions and calibrate — written context only tells me what someone decided to write down. A real exchange lets me test my understanding and surface what I did not know I needed to ask.",
                [new("upfront_clarity_need",2),new("verbal_alignment_preference",2),new("clarity_via_written_context",-1)]),
            new('C',"Seeing examples or trying a small piece of real work — I learn more about how a collaboration works by doing something together than by discussing it first.",
                [new("learning_by_doing",2),new("ambiguity_tolerance",2),new("upfront_clarity_need",-1)]),
            new('D',"Clear ownership and decision boundaries outlined somewhere, and discussion about them when needed — I can move quickly once I know who is accountable for what and where my judgment applies.",
                [new("ownership_boundary_clarity",2),new("upfront_clarity_need",2)]),
        ]),
        new(1,2,2,"After initial alignment, what source of truth helps you most?",
        [
            new('A',"A written summary of decisions, responsibilities, and next steps — I need to be able to return to what we agreed without relying on memory or asking again.",
                [new("clarity_via_written_context",2),new("verbal_alignment_preference",-1)]),
            new('B',"A shared board or plan with priorities, owners, and progress — I want a live view of what is happening, not just a snapshot from the last meeting.",
                [new("visibility_preference",2),new("clarity_via_written_context",1)]),
            new('C',"Clear task ownership in the work-tracking system — I work best when responsibility is attached directly to the task, not discussed separately.",
                [new("ownership_boundary_clarity",2),new("clarity_via_written_context",1)]),
            new('D',"I usually rely more on conversation than written tracking — I find that regular dialogue keeps me aligned more reliably than maintaining documents.",
                [new("verbal_alignment_preference",2),new("clarity_via_written_context",-1)]),
        ]),
        new(1,3,3,"When you join an existing team or project, what helps you integrate fastest?",
        [
            new('A',"Documentation, previous decisions, and project context — I want to understand what was tried, decided, and why before I start forming my own view.",
                [new("clarity_via_written_context",2),new("upfront_clarity_need",1)]),
            new('B',"A conversation with someone who knows the history — written context only goes so far. I learn fastest by talking to someone who can answer what documents cannot.",
                [new("verbal_alignment_preference",2),new("clarity_via_written_context",-1)]),
            new('C',"Looking at real examples: tasks, code, tickets, or past work — the actual work tells me more about how a team operates than any description of it.",
                [new("learning_by_doing",2),new("clarity_via_artifact_context",2),new("ambiguity_tolerance",1)]),
            new('D',"Understanding team norms, roles, and expectations — I integrate faster when I know how the team works together and what is expected of me, not just what the work is.",
                [new("social_context_need",2),new("ownership_boundary_clarity",2)]),
        ]),
        new(1,4,4,"When working with someone new, which situation would make you most cautious?",
        [
            new('A',"No one is clear who owns the decision or next step — unclear ownership creates duplication, dropped work, and confusion about who to go to when something needs resolving.",
                [new("ownership_boundary_clarity",2)]),
            new('B',"Plans change verbally, but the change is not visible anywhere — changes that never land in the system are easy to forget and hard to hold anyone to.",
                [new("clarity_via_written_context",2),new("visibility_preference",1)]),
            new('C',"People act on assumptions without checking understanding — decisions are made on things nobody actually agreed on.",
                [new("explicit_alignment_need",2)]),
            new('D',"Risks or mistakes are not raised early — I find it harder to trust a collaboration where problems are held back until they become urgent.",
                [new("risk_transparency_need",2)]),
        ]),
        // ── Section 2 ───────────────────────────────────────────────────
        new(2,1,5,"During shared work, when do you prefer alignment conversations about goals, responsibilities, progress, and changes to happen?",
        [
            new('A',"At scheduled check-ins, so people can prepare and avoid unnecessary interruptions — I find that alignment works better when people can come ready, not pulled in mid-thought.",
                [new("scheduled_communication_preference",2),new("focus_protection",1)]),
            new('B',"Spontaneously, whenever alignment needs attention — I would rather address a misalignment the moment I notice it than wait for a scheduled moment that may not come soon enough.",
                [new("immediate_alignment_preference",2),new("verbal_alignment_preference",1)]),
            new('C',"Scheduled only when there is a clear reason, such as a blocker, decision, or major change — conversations without a purpose tend to create more noise than clarity.",
                [new("focus_protection",2),new("scheduled_communication_preference",1)]),
            new('D',"At a regular rhythm, with spontaneous conversations for urgent issues — I want a predictable baseline with room to escalate when something cannot wait.",
                [new("scheduled_communication_preference",1),new("immediate_alignment_preference",1),new("verbal_alignment_preference",1)]),
        ]),
        new(2,2,6,"During shared work, where should important updates, decisions, and progress be captured?",
        [
            new('A',"In a written summary or decision note — I need decisions separated from the noise of the thread so I can find and reference them later.",
                [new("clarity_via_written_context",2),new("verbal_alignment_preference",-1)]),
            new('B',"On a shared board or plan with owners and progress — I want the state of the work visible in one place, not scattered across messages.",
                [new("visibility_preference",2),new("clarity_via_written_context",1)]),
            new('C',"In task comments, tickets, or pull requests close to the work — context belongs next to what it describes, not in a separate document that may drift.",
                [new("clarity_via_artifact_context",2),new("clarity_via_written_context",1)]),
            new('D',"In conversation first; written capture is only needed when something is unclear, risky, or important — I do not think everything needs to be documented, only the things worth coming back to.",
                [new("verbal_alignment_preference",2),new("clarity_via_written_context",-1)]),
        ]),
        new(2,3,7,"During shared work, how much visibility do you prefer into collaborators' progress before the work is finished?",
        [
            new('A',"Frequent small updates while work is still in progress — whether through a board, messages, or brief updates, I find it easier to coordinate and help when I have a live sense of where things are, not just the final result.",
                [new("visibility_preference",2),new("immediate_alignment_preference",1)]),
            new('B',"Updates when progress, blockers, or assumptions meaningfully change — I do not need a running feed, just a signal when something shifts that I should know about.",
                [new("visibility_preference",1),new("risk_transparency_need",1)]),
            new('C',"Updates at agreed checkpoints, such as review points or milestones — predictable moments work better for me than continuous updates that interrupt flow.",
                [new("focus_protection",2),new("scheduled_communication_preference",1)]),
            new('D',"Minimal updates unless help, coordination, or a decision is needed — I trust people to surface what matters and prefer not to monitor what does not require my attention.",
                [new("ambiguity_tolerance",2),new("focus_protection",1)]),
        ]),
        // ── Section 3 ───────────────────────────────────────────────────
        new(3,1,8,"Which way of organizing work feels most natural to you?",
        [
            new('A',"Fixed cycles with planning, review, and reflection — I work best when there is a predictable rhythm with clear moments to plan, deliver, and improve.",
                [new("iteration_preference",2),new("scheduled_communication_preference",1)]),
            new('B',"Continuous flow through a visible board or backlog — I prefer to pull from a prioritized list and keep work moving without waiting for the next cycle to start. The backlog doesn't need to be complete before I begin.",
                [new("flow_preference",2),new("ambiguity_tolerance",1)]),
            new('C',"A hybrid approach with some planning rhythm and flexible flow — I want the predictability of regular planning without the rigidity of fixed cycles that cannot respond to change.",
                [new("iteration_preference",1),new("flow_preference",1)]),
            new('D',"Larger upfront planning with milestones and dependencies clarified early — I find it easier to execute when the full scope, sequence, and dependencies are visible from the start.",
                [new("upfront_planning_preference",2),new("upfront_clarity_need",1)]),
        ]),
        new(3,2,9,"Before starting a larger piece of work, what helps you plan most effectively?",
        [
            new('A',"Clarifying scope, milestones, dependencies, and risks upfront — I need the full picture before I start so I can move confidently without discovering surprises mid-execution.",
                [new("upfront_planning_preference",2),new("upfront_clarity_need",1)]),
            new('B',"Creating a prioritized backlog with enough detail for the next step — I plan just enough to start well and let the rest emerge as I learn more.",
                [new("flow_preference",2),new("ambiguity_tolerance",1)]),
            new('C',"Starting with a rough direction and refining the plan as we learn — I find that too much planning before we understand the problem creates false certainty.",
                [new("adaptive_planning_preference",2),new("ambiguity_tolerance",2),new("upfront_planning_preference",-1)]),
            new('D',"When uncertainty or risk is high, running a small experiment first and planning based on what we learn — I would rather build the plan on something real than commit to assumptions we have not tested.",
                [new("evidence_based_planning_preference",2),new("risk_transparency_need",1),new("ambiguity_tolerance",1)]),
        ]),
        new(3,3,10,"When priorities change during shared work, what makes the change easiest for you to work with?",
        [
            new('A',"The responsible person updates the backlog, board, or plan clearly — a priority change that only lives in conversation has not really been made yet.",
                [new("clarity_via_written_context",2),new("visibility_preference",1),new("verbal_alignment_preference",-1)]),
            new('B',"The reason, trade-offs, and risks are explained before the change is made — I can adapt more easily when I understand what was considered and what we are accepting.",
                [new("risk_transparency_need",2),new("tradeoff_visibility",1)]),
            new('C',"The change is discussed with the people affected before it is finalized — I need to be part of the conversation before the change lands, not just informed after.",
                [new("participatory_decision_preference",2),new("explicit_alignment_need",1)]),
            new('D',"Changes are grouped into planning points rather than introduced continuously — I find it harder to work well when priorities can shift at any moment without a boundary.",
                [new("planning_boundary_protection",2),new("focus_protection",1),new("scheduled_communication_preference",1)]),
        ]),
        new(3,4,11,"When a team has already committed to a plan, how should new requests be handled?",
        [
            new('A',"Add them only if the team explicitly removes or deprioritizes existing work — if we are taking on something new, we need to be honest about what we are no longer doing.",
                [new("capacity_protection",2),new("sustainable_pace_preference",1)]),
            new('B',"Decide case by case after discussing urgency, impact, and risks — I am open to changing the plan, but I want the decision to be conscious, not reactive.",
                [new("tradeoff_visibility",2),new("risk_transparency_need",1),new("participatory_decision_preference",1),new("change_tolerance",1)]),
            new('C',"Add them flexibly if they are clearly valuable — I think the ability to respond to something important should not be blocked by process.",
                [new("change_tolerance",2),new("ambiguity_tolerance",1),new("capacity_protection",-1),new("sustainable_pace_preference",-1)]),
            new('D',"Save them for the next planning point unless they are truly urgent — I need the plan to mean something, and that requires protecting it from constant interruption.",
                [new("planning_boundary_protection",2),new("capacity_protection",1),new("sustainable_pace_preference",1)]),
        ]),
        new(3,5,12,"If priorities or plans are being managed by someone else, what do you rely on them for most?",
        [
            new('A',"Keeping the backlog, board, or plan up to date — I need the system to reflect what is actually true, not what was decided two weeks ago.",
                [new("clarity_via_written_context",2),new("visibility_preference",2)]),
            new('B',"Making trade-offs, risks, and alternative paths visible — I want to understand not just what was decided but what was considered and what we are giving up.",
                [new("tradeoff_visibility",2),new("risk_transparency_need",2)]),
            new('C',"Protecting focus by limiting unplanned changes — I rely on whoever holds the plan to push back on requests that would disrupt work already in progress.",
                [new("focus_protection",2),new("planning_boundary_protection",2)]),
            new('D',"Involving the team before changing scope, timing, or responsibilities — I expect that changes affecting my work or ownership will include me before they are finalized.",
                [new("participatory_decision_preference",2),new("ownership_boundary_clarity",1)]),
        ]),
        new(3,6,13,"When a team needs to improve how it works, what approach feels most useful to you?",
        [
            new('A',"Regular retrospectives or reflection sessions — I think improvement needs dedicated time to happen. Without a structured moment, it tends to get crowded out by delivery.",
                [new("scheduled_communication_preference",2),new("iteration_preference",1)]),
            new('B',"Small improvements whenever a problem appears — I prefer to fix things in the moment rather than accumulate problems until the next formal session.",
                [new("immediate_alignment_preference",2),new("ambiguity_tolerance",1)]),
            new('C',"Looking at evidence such as delivery patterns, quality issues, incidents, or feedback — I trust observable signals more than opinions when deciding where to improve.",
                [new("root_cause_orientation",2),new("impact_calibration_preference",1)]),
            new('D',"Informal conversations when something feels off — I find that honest conversations between the right people often resolve things faster than any formal process.",
                [new("verbal_alignment_preference",2),new("relational_connection_need",1)]),
        ]),
        new(3,7,14,"When a team reflects on how work is going, what topics should be included?",
        [
            new('A',"Work process, planning, blockers, and delivery flow — I think the most important thing to examine is whether the team can execute and deliver reliably.",
                [new("delivery_reflection_priority",2)]),
            new('B',"Technical quality, incidents, risks, and maintainability — I think reflection should include whether the work we are producing is actually sound, not just delivered on time.",
                [new("technical_reflection_priority",2)]),
            new('C',"Collaboration patterns, communication, and decision-making — these topics feel less urgent than delivery, but left unexamined they tend to create friction that affects people and eventually shows up in the work.",
                [new("interpersonal_reflection_priority",2)]),
            new('D',"Team safety, pressure, conflict, and how people are treated — when people do not feel safe being honest, real problems stay hidden until they are already affecting the work. That is usually when they are hardest to fix.",
                [new("safety_reflection_priority",2)]),
        ]),
        // ── Section 4 ───────────────────────────────────────────────────
        new(4,1,15,"When work is under time pressure, what most helps you feel comfortable calling it done or handing it over?",
        [
            new('A',"It solves the core problem reliably. Risks, reviews, and follow-up plans matter, but they should not always be the gate.",
                [new("pragmatic_completion_preference",2),new("ambiguity_tolerance",1)]),
            new('B',"I have a clear picture of what is still open and can make it visible — knowing and naming the gaps is what makes handing over feel responsible to me.",
                [new("risk_transparency_need",2),new("visibility_preference",2)]),
            new('C',"Someone else has reviewed it. I trust the handoff more when another person has checked the work or challenged my assumptions.",
                [new("external_validation_need",2)]),
            new('D',"What is not done now has a clear owner and plan. Iteration is fine, but only with accountability.",
                [new("ownership_boundary_clarity",2),new("upfront_clarity_need",1)]),
        ]),
        new(4,2,16,"You discover a problem with work that has already been handed off or shared. What is your instinct?",
        [
            new('A',"I tell the relevant people early, even before I know everything. Surprises are worse than uncertainty.",
                [new("risk_transparency_need",2),new("immediate_alignment_preference",1)]),
            new('B',"I assess how serious it is first. A proportionate response serves everyone better than immediate alarm.",
                [new("impact_calibration_preference",2),new("ambiguity_tolerance",1)]),
            new('C',"I try to fix it quickly before involving others. If I can resolve it responsibly, I do not want to create unnecessary noise.",
                [new("autonomous_problem_resolution",2),new("ambiguity_tolerance",1)]),
            new('D',"I look for why it happened, not only how to fix it. Preventing the same problem matters as much as resolving this one.",
                [new("root_cause_orientation",2),new("risk_transparency_need",1)]),
        ]),
        new(4,3,17,"Your team needs to reduce scope or quality to meet a deadline. What is your instinct?",
        [
            new('A',"I make the compromise visible and documented, but I do not necessarily seek agreement. Transparency is enough for me.",
                [new("risk_transparency_need",2),new("clarity_via_written_context",1),new("participatory_decision_preference",-1)]),
            new('B',"I protect the quality bar, even if it means pushing back on the timeline or scope reduction. Some risks are not worth accepting.",
                [new("quality_protection_preference",2),new("sustainable_pace_preference",1)]),
            new('C',"I accept the compromise and trust we will improve later. Progress over perfection.",
                [new("pragmatic_completion_preference",2),new("change_tolerance",1)]),
            new('D',"I need explicit agreement from the people affected before moving forward. Alignment is not optional.",
                [new("participatory_decision_preference",2),new("explicit_alignment_need",1)]),
        ]),
        new(4,4,18,"You are starting a significant piece of work, and the expected quality bar has not been clearly defined. What is your instinct?",
        [
            new('A',"I clarify what 'good enough' means before going too far. Shared expectations prevent rework.",
                [new("upfront_clarity_need",2),new("explicit_alignment_need",1)]),
            new('B',"I define my own quality bar and make my assumptions visible. Someone needs to create a starting point.",
                [new("ownership_boundary_clarity",2),new("ambiguity_tolerance",1)]),
            new('C',"I start with a smaller version and use feedback to calibrate the quality bar. Learning through progress is safest.",
                [new("adaptive_planning_preference",2),new("learning_by_doing",1)]),
            new('D',"I use existing team standards or past examples as the quality bar. Consistency matters when expectations are unclear.",
                [new("clarity_via_artifact_context",2),new("upfront_clarity_need",1)]),
        ]),
        new(4,5,19,"Someone finds a problem in work you considered done. What is your instinct?",
        [
            new('A',"I acknowledge it and focus on fixing it. If there is a real problem, ownership matters more than explanation.",
                [new("accountability_preference",2)]),
            new('B',"I first assess how much it matters. The right response depends on actual impact, not the finding itself.",
                [new("impact_calibration_preference",2)]),
            new('C',"I want to understand why it is considered a problem. Not every concern reflects a real gap.",
                [new("critical_feedback_validation",2)]),
            new('D',"I want to revisit expectations and assumptions. If the quality bar was not shared clearly, the finding itself is worth examining.",
                [new("explicit_alignment_need",2),new("upfront_clarity_need",1)]),
        ]),
        // ── Section 5 ───────────────────────────────────────────────────
        new(5,1,20,"Someone tells you that your feedback was too harsh, but you believe the concern you raised was valid. What is your instinct?",
        [
            new('A',"I focus on whether my feedback was accurate; useful feedback should not be dismissed because it was uncomfortable.",
                [new("feedback_content_primacy_giving",2),new("feedback_delivery_awareness_giving",-1)]),
            new('B',"I adjust my delivery because feedback only works if the other person can actually use it.",
                [new("feedback_delivery_awareness_giving",2),new("feedback_content_primacy_giving",-1)]),
            new('C',"I want to separate the two issues: whether the concern was valid, and whether the delivery made it harder to hear.",
                [new("feedback_content_primacy_giving",1),new("feedback_delivery_awareness_giving",1)]),
            new('D',"I ask what specifically felt harsh so we can agree on a better way to discuss concerns next time.",
                [new("communication_norm_alignment",2)]),
        ]),
        new(5,2,21,"You receive feedback that feels unnecessarily harsh, but the concern may be valid. What is your instinct?",
        [
            new('A',"I try to separate the message from the delivery. If there is something useful in the feedback, I want to understand it even if the style was poor.",
                [new("feedback_content_primacy_receiving",2),new("feedback_delivery_awareness_receiving",-1)]),
            new('B',"I ask for the feedback to be given in a way I can actually use. Directness is okay, but the delivery needs to support the conversation, not make it harder.",
                [new("feedback_delivery_awareness_receiving",2),new("feedback_content_primacy_receiving",-1)]),
            new('C',"I want to discuss both the concern and how it was delivered. The feedback itself may matter, but so does the communication pattern around it.",
                [new("feedback_content_primacy_receiving",1),new("feedback_delivery_awareness_receiving",1),new("communication_norm_alignment",1)]),
            new('D',"I question the feedback more if the delivery feels unclear, exaggerated, or unfair. If the style distorts the message, I need to examine whether the concern is valid.",
                [new("critical_feedback_validation",2),new("feedback_delivery_awareness_receiving",1),new("feedback_content_primacy_receiving",-1)]),
        ]),
        new(5,3,22,"When you have a concern about someone's work or behavior, what is your default instinct?",
        [
            new('A',"I raise it privately first. Public discussion should be a last resort.",
                [new("private_feedback_preference",2)]),
            new('B',"I raise it where the relevant people can see or hear it. Transparency helps the team understand and respond to problems together.",
                [new("visibility_preference",2),new("private_feedback_preference",-1)]),
            new('C',"I adjust based on the person and relationship. Some people can handle public discussion; others need more care or privacy.",
                [new("social_context_need",2)]),
            new('D',"I raise it in the context where the concern appeared. If it happened in a meeting, task, review, or shared discussion, I usually address it there.",
                [new("immediate_alignment_preference",2)]),
        ]),
        new(5,4,23,"Someone raises a concern about your work or behavior in front of others. What matters most to you?",
        [
            new('A',"That the concern is relevant to the group. If others are affected or need the context, public discussion can be appropriate.",
                [new("visibility_preference",2)]),
            new('B',"That it is handled respectfully and without embarrassment. Public feedback can be okay, but not if it undermines trust or dignity.",
                [new("dignity_in_feedback_need",2)]),
            new('C',"That I have a chance to respond or clarify context. If a concern is raised publicly, I need room for my perspective too.",
                [new("participatory_decision_preference",2)]),
            new('D',"That the person follows up privately afterward. Public discussion may solve the immediate issue, but repair often needs a separate conversation.",
                [new("relational_repair_need",2)]),
        ]),
        // ── Section 6 ───────────────────────────────────────────────────
        new(6,1,24,"What starts feeling like micromanagement to you?",
        [
            new('A',"Frequent status requests without a clear reason. I do not mind sharing progress, but repeated check-ins feel frustrating when they do not change anything or help the work.",
                [new("autonomous_pace_preference",2),new("focus_protection",1)]),
            new('B',"Detailed direction on work I already own. Once I am responsible for something, too much instruction about how to do it can feel like my judgment is not trusted.",
                [new("autonomous_execution_preference",2)]),
            new('C',"Decisions being reopened repeatedly after alignment. If we already agreed on direction, repeated second-guessing makes ownership feel unstable.",
                [new("planning_boundary_protection",2),new("ownership_boundary_clarity",1)]),
            new('D',"Oversight that feels more like control than support. I can accept involvement, but not when it seems driven by anxiety, distrust, or a need to approve every step.",
                [new("support_over_control_preference",2)]),
        ]),
        new(6,2,25,"What starts feeling like abandonment to you?",
        [
            new('A',"Expectations change without discussion or support. I can handle change, but not when I am left to absorb it alone or guess what is now expected.",
                [new("participatory_decision_preference",2),new("explicit_alignment_need",1)]),
            new('B',"Important decisions become hard to get help on. I can work independently, but blocked decisions or unavailable guidance make me feel unsupported.",
                [new("unblocking_support_need",2)]),
            new('C',"I only hear feedback when something goes wrong. If there is no encouragement, calibration, or signal until a problem appears, support starts to feel absent.",
                [new("proactive_feedback_need",2)]),
            new('D',"Coordination disappears even though the work is interdependent. Autonomy is fine, but connected work still needs communication, timing, and shared awareness.",
                [new("visibility_preference",2),new("scheduled_communication_preference",1),new("upfront_planning_preference",1)]),
        ]),
        new(6,3,26,"When you are unsure how to move forward, what kind of support feels most helpful?",
        [
            new('A',"Help clarifying the goal, constraints, and what matters most. I can move forward once the direction is clearer.",
                [new("upfront_clarity_need",2),new("explicit_alignment_need",1),new("upfront_planning_preference",1)]),
            new('B',"A concrete decision or recommendation from someone with context. I do not want to stay blocked when someone else can unblock the path.",
                [new("directive_support_preference",2),new("ambiguity_tolerance",-1)]),
            new('C',"A conversation to think through options together. I value support that helps me reason, not just gives me an answer.",
                [new("verbal_alignment_preference",2),new("participatory_decision_preference",1)]),
            new('D',"Space to explore first, then bring back what I learn. I prefer not to involve others before I have formed my own view.",
                [new("ambiguity_tolerance",2),new("autonomous_problem_resolution",1),new("learning_by_doing",1),new("directive_support_preference",-1)]),
        ]),
        new(6,4,27,"What makes ownership of work feel healthy to you?",
        [
            new('A',"I know what decisions are mine to make. Clear boundaries make ownership feel safe.",
                [new("ownership_boundary_clarity",2)]),
            new('B',"I can ask for help without it being seen as a lack of competence. Support should not reduce trust.",
                [new("help_seeking_safety_need",2)]),
            new('C',"Others stay informed enough that my work does not become isolated. Ownership should not mean disappearing.",
                [new("visibility_preference",2),new("scheduled_communication_preference",1)]),
            new('D',"I have enough freedom to make trade-offs without needing approval for every step. Ownership should come with real authority.",
                [new("autonomous_execution_preference",2),new("ownership_boundary_clarity",1)]),
        ]),
        // ── Section 7 ───────────────────────────────────────────────────
        new(7,1,28,"What kind of a scheduled status meeting feels most useful?",
        [
            new('A',"Short, structured, and focused on blockers — I want to leave knowing what is moving and what is not, not having discussed everything.",
                [new("focus_protection",2),new("visibility_preference",1)]),
            new('B',"Discussion-based, with space for context — sometimes the status itself is less useful than understanding why things are the way they are.",
                [new("verbal_alignment_preference",2),new("tradeoff_visibility",1)]),
            new('C',"Mostly async updates, with meetings only when something actually needs live discussion — the default should be written, not scheduled.",
                [new("focus_protection",2),new("clarity_via_written_context",2),new("verbal_alignment_preference",-1)]),
            new('D',"A regular rhythm, even if there is not always much to discuss — the consistency itself has value, and I would rather have a quiet meeting than lose the habit.",
                [new("scheduled_communication_preference",2),new("social_context_need",1)]),
        ]),
        new(7,2,29,"What is the strongest justification for interrupting someone with an unscheduled conversation?",
        [
            new('A',"Something is genuinely blocked and every hour of waiting has a real cost. I would rather interrupt than let the work stall or the decision stay open longer than it needs to.",
                [new("immediate_alignment_preference",2),new("focus_protection",-1)]),
            new('B',"The topic is too sensitive or too easily misread in writing. Some things need tone, expression, and real-time response — not a message that sits there being interpreted.",
                [new("verbal_alignment_preference",2),new("social_context_need",1)]),
            new('C',"The written thread has already taken longer than a short conversation would. When async stops being efficient, switching to real-time is not an interruption — it is the right call.",
                [new("verbal_alignment_preference",1),new("pragmatic_completion_preference",1)]),
            new('D',"Honestly, almost nothing justifies it by default. Most things can wait, and I think respecting someone's focus means letting them choose when they are available rather than pulling them out of it.",
                [new("focus_protection",2),new("scheduled_communication_preference",1),new("verbal_alignment_preference",-1)]),
        ]),
        new(7,3,30,"If a recurring meeting could only guarantee one of these things, which would make it worth keeping?",
        [
            new('A',"A clear agenda before it starts and a visible outcome, decision, or next step after. Without those two things I struggle to justify the time, no matter how often we meet.",
                [new("upfront_clarity_need",2),new("visible_result_need",1),new("focus_protection",1)]),
            new('B',"The team actually being in the same space regularly. Some of what keeps collaboration working is not output — it is familiarity, rhythm, and the kind of connection that does not happen in tasks and tickets.",
                [new("relational_connection_need",2),new("scheduled_communication_preference",1)]),
            new('C',"Shared awareness of what is shifting, what is blocked, and what people need from each other. The cost of misalignment is usually higher than the cost of the meeting itself.",
                [new("explicit_alignment_need",2),new("visibility_preference",1),new("risk_transparency_need",1)]),
            new('D',"The genuine freedom to cancel it when it is not needed. A meeting that earns its place every time it happens feels very different from one that just recurs because it was scheduled.",
                [new("focus_protection",2)]),
        ]),
        new(7,4,31,"What makes a meeting feel draining or unnecessary?",
        [
            new('A',"No clear decision, outcome, or next step came from it — I can accept that a meeting was hard or long, but I struggle when I cannot point to what it produced.",
                [new("visible_result_need",2),new("focus_protection",1)]),
            new('B',"The same context could have been shared in writing and people could have responded in their own time — synchronous time is expensive and I notice when it is not actually needed.",
                [new("clarity_via_written_context",2),new("focus_protection",1),new("verbal_alignment_preference",-1)]),
            new('C',"Too many people were included without a clear reason to be there — it signals either unclear ownership or a habit of over-involving people as a substitute for actual communication.",
                [new("ownership_boundary_clarity",2)]),
            new('D',"It pulled me out of focused work without delivering enough value to justify the cost — the interruption itself is the problem, not just the meeting content.",
                [new("focus_protection",2)]),
        ]),
        // ── Section 8 ───────────────────────────────────────────────────
        new(8,1,32,"When work becomes urgent, what kind of communication helps you most?",
        [
            new('A',"A calm, clear statement of what matters most right now and what can wait — I do not need the urgency performed, I need the priority to be unambiguous so I can move without second-guessing.",
                [new("calm_urgency_preference",2),new("upfront_clarity_need",1)]),
            new('B',"Enough context to understand why it is urgent and what is actually at stake — urgency without reasoning makes it hard for me to make good decisions about how to respond.",
                [new("risk_transparency_need",2),new("tradeoff_visibility",1)]),
            new('C',"A quick collaborative moment to agree on what gets dropped or deferred — I can move fast once we are honest about what we are not doing anymore.",
                [new("participatory_decision_preference",2),new("immediate_alignment_preference",1)]),
            new('D',"A written summary I can come back to — when things are urgent, live conversation creates noise I have to carry in my head. Written expectations let me focus instead of remember.",
                [new("clarity_via_written_context",2),new("focus_protection",1),new("verbal_alignment_preference",-1)]),
        ]),
        new(8,2,33,"Your manager addresses the team before a critical deadline. Which would actually motivate you most?",
        [
            new('A',"\"If we don't deliver this, heads will roll.\"",
                [new("pressure_driven_motivation",2),new("calm_urgency_preference",-1)]),
            new('B',"\"This is not university. We have to deliver.\"",
                [new("pressure_driven_motivation",2),new("calm_urgency_preference",-1)]),
            new('C',"\"I think this is a genuinely exciting milestone. Let me know if anything is blocking you.\"",
                [new("positive_motivation_preference",2),new("calm_urgency_preference",1)]),
            new('D',"Nothing — the deadline is visible, the priorities are clear. A speech would mostly get in the way.",
                [new("autonomous_motivation",2),new("focus_protection",1)]),
        ]),
        new(8,3,34,"When a manager or colleague communicates urgency in a way that bothers you, what do you usually do?",
        [
            new('A',"I raise it directly with the person — not necessarily in the moment, but soon after. I think patterns like that are worth naming even when it is uncomfortable.",
                [new("direct_conflict_approach",2),new("private_feedback_preference",1)]),
            new('B',"I absorb it and focus on the work — I let the communication style go and do not raise it unless it becomes a repeated pattern that is genuinely affecting the team.",
                [new("communication_friction_tolerance",2),new("team_impact_orientation",1)]),
            new('C',"I mention it to someone I trust, but not to the person directly — sometimes naming it out loud is enough, even without confrontation.",
                [new("conflict_avoidance_tendency",2),new("indirect_conflict_processing",1)]),
            new('D',"It depends on the relationship and how often it happens — a one-off I can usually let go, but if it keeps happening I will say something.",
                [new("direct_conflict_approach",1),new("social_context_need",1)]),
        ]),
        new(8,4,35,"During a high-pressure or urgent situation, someone tells you that your tone, word choice, or communication style felt too intense, too blunt, or hard to receive. What is your instinct?",
        [
            new('A',"I take it seriously and adjust — if my delivery made it harder for someone to think clearly or act well, that is my problem to fix regardless of the pressure we were both under.",
                [new("feedback_delivery_awareness_giving",2),new("accountability_preference",1)]),
            new('B',"I want to understand what specifically landed badly before I change anything — even under pressure, adjusting without understanding what went wrong usually does not help either of us.",
                [new("root_cause_orientation",2),new("feedback_delivery_awareness_giving",1)]),
            new('C',"I acknowledge the reaction but do not automatically see it as something to fix — sometimes intensity reflects the situation honestly, and I think that can be said without apologizing for it.",
                [new("feedback_content_primacy_giving",2),new("critical_feedback_validation",1)]),
            new('D',"I notice whether it is a pattern or a one-off — pressure situations are not normal conditions, and a single reaction does not always mean I need to change how I communicate generally.",
                [new("impact_calibration_preference",2)]),
        ]),
        // ── Section 9 ───────────────────────────────────────────────────
        new(9,1,36,"You notice tension between yourself and a colleague. What feels most important to you first?",
        [
            new('A',"Understand what the disagreement actually is — the feeling of tension is not always a reliable signal of what is really wrong, and I want to know what I am actually dealing with before responding to it.",
                [new("impact_calibration_preference",2),new("root_cause_orientation",1)]),
            new('B',"Make sure it does not affect the wider team — tension between two people has a way of changing the atmosphere for everyone, and I feel responsible for containing that boundary early.",
                [new("team_impact_orientation",2),new("private_feedback_preference",1)]),
            new('C',"Give both of us some space before trying to address it — when people are still in the feeling of tension, conversations tend to produce reactions rather than resolution.",
                [new("tension_deescalation_preference",2)]),
            new('D',"Name it directly before it has time to harden — the longer something sits unacknowledged between two people, the more weight it accumulates.",
                [new("direct_conflict_approach",2),new("immediate_alignment_preference",1)]),
        ]),
        new(9,2,37,"When should an issue be escalated?",
        [
            new('A',"As soon as delivery or coordination is genuinely at risk — waiting for the right process when something real is breaking feels like prioritising comfort over responsibility.",
                [new("issue_escalation_preference",2),new("team_impact_orientation",1),new("immediate_alignment_preference",1),new("risk_transparency_need",1)]),
            new('B',"When the issue involves a decision or authority that neither person actually has — some conflicts cannot be resolved directly because the people involved do not have the power to resolve them, and escalation in that case is not avoidance, it is the right path.",
                [new("issue_escalation_preference",2),new("ownership_boundary_clarity",1)]),
            new('C',"When the same pattern repeats without improvement — a single incident rarely justifies escalation, but a pattern that nobody is addressing is a different problem entirely.",
                [new("root_cause_orientation",2),new("impact_calibration_preference",1),new("communication_friction_tolerance",1),new("issue_escalation_preference",1)]),
            new('D',"Only when the people involved have genuinely tried and cannot resolve it responsibly on their own — escalation should be a last resort, not a shortcut around a difficult conversation.",
                [new("direct_conflict_approach",2),new("autonomous_problem_resolution",1)]),
        ]),
        new(9,3,38,"When a difficult conversation is necessary, what do you most want it to produce?",
        [
            new('A',"A clear decision or resolution — I can handle hard conversations well when they end with something concrete. Ambiguity after a difficult exchange is harder for me than the conversation itself.",
                [new("visible_result_need",2)]),
            new('B',"Mutual understanding of how each person sees it — I do not always need agreement, but I need to feel that both perspectives were genuinely heard before we move on.",
                [new("mutual_understanding_need",2)]),
            new('C',"The relationship staying intact and workable — the outcome matters, but not more than the two people being able to collaborate afterwards without it hanging over everything.",
                [new("relational_connection_need",2),new("relational_repair_need",1)]),
            new('D',"Both people owning something — I find conversations more honest when neither person is only delivering or only receiving. Shared accountability changes the dynamic entirely.",
                [new("shared_accountability_preference",2),new("accountability_preference",1)]),
        ]),
        new(9,4,39,"When tension or conflict between you and a colleague is not resolving itself, would you want someone to help facilitate, and who should that be?",
        [
            new('A',"Yes — a neutral peer who knows both of us and has enough context to help without taking sides.",
                [new("facilitation_openness",2),new("social_context_need",1)]),
            new('B',"Yes — a manager or someone with enough authority to move things forward if the conversation stalls.",
                [new("facilitation_openness",2),new("issue_escalation_preference",2)]),
            new('C',"Only if both people agree to it — facilitation imposed on one side usually makes things worse, not better.",
                [new("explicit_alignment_need",2),new("facilitation_openness",1)]),
            new('D',"No — I think bringing in a third party changes the dynamic in ways that are hard to undo. I would rather resolve it directly even if it takes longer.",
                [new("direct_conflict_approach",2),new("autonomous_problem_resolution",1)]),
        ]),
        new(9,5,40,"When conflict or tension appears in a team, what do you believe about how it resolves?",
        [
            new('A',"It needs active intervention — left alone, tension rarely improves and usually hardens into something more difficult to address later.",
                [new("direct_conflict_approach",2),new("risk_transparency_need",1)]),
            new('B',"It depends on the people involved — not everyone processes conflict the same way, and assuming everyone needs the same kind of conversation, space, or ritual usually creates its own friction.",
                [new("social_context_need",2),new("ambiguity_tolerance",1)]),
            new('C',"It often resolves itself if people are given enough space and time — premature intervention can make people feel managed rather than trusted.",
                [new("tension_deescalation_preference",2),new("autonomous_problem_resolution",1)]),
            new('D',"It resolves more reliably when the team has regular practices that make tension safe to surface early — not just when something breaks. Without those habits, people wait too long or avoid it entirely.",
                [new("communication_norm_alignment",2),new("proactive_conflict_culture_preference",1),new("scheduled_communication_preference",1)]),
        ]),
        new(9,6,41,"You are in a leading role, and a team member comes to you and tells you they are in conflict with a colleague. What is your instinct?",
        [
            new('A',"I listen first and ask what they have already tried — I want to understand the situation before doing anything, and I want to support them in resolving it directly if that is still possible.",
                [new("autonomous_problem_resolution",2),new("impact_calibration_preference",1)]),
            new('B',"I offer to facilitate a conversation or organize a moderation between both people — bringing them together in a structured way feels more useful than handling each side separately.",
                [new("facilitation_openness",2)]),
            new('C',"I talk to the other person separately to hear their perspective before deciding anything — one account is never the whole picture.",
                [new("impact_calibration_preference",2),new("social_context_need",1)]),
            new('D',"I assess whether this is affecting the wider team before deciding how to respond — a conflict between two people is sometimes a symptom of something bigger, and I want to understand that before focusing only on the two people involved.",
                [new("team_impact_orientation",2),new("root_cause_orientation",1)]),
        ]),
        // ── Section 10 ──────────────────────────────────────────────────
        new(10,1,42,"What motivates you most in your work?",
        [
            new('A',"The craft itself — I find genuine satisfaction in building things well, solving hard problems, and improving how things work. The work is its own reward.",
                [new("craft_intrinsic_motivation",2),new("autonomous_motivation",1)]),
            new('B',"Understanding the purpose — I need to know why something matters. Motivation drops significantly when I cannot connect the work to a meaningful outcome.",
                [new("purpose_driven_motivation",2)]),
            new('C',"Ownership and trust — being given real responsibility and the freedom to make decisions is what makes work feel worth doing.",
                [new("autonomy_driven_motivation",2),new("autonomous_execution_preference",1)]),
            new('D',"The people and relationships — the team, the collaboration, the shared experience of building something together matters more to me than the work itself.",
                [new("relational_driven_motivation",2),new("relational_connection_need",1)]),
        ]),
        new(10,2,43,"What do you want more of in your work?",
        [
            new('A',"Deep focus time — uninterrupted stretches where I can actually think, not just react to whatever is most urgent.",
                [new("focus_protection",2),new("autonomous_pace_preference",1)]),
            new('B',"Clearer priorities — not more information, just a more honest signal about what actually matters most right now.",
                [new("upfront_clarity_need",2)]),
            new('C',"Better collaboration and feedback — work that involves real exchange, not just parallel effort with occasional updates.",
                [new("proactive_feedback_need",2),new("verbal_alignment_preference",1)]),
            new('D',"More ownership and decision-making space — I want to be trusted with the whole problem, not just the execution of someone else's solution.",
                [new("autonomy_driven_motivation",2),new("autonomous_execution_preference",1)]),
        ]),
        new(10,3,44,"What kind of recognition feels most meaningful?",
        [
            new('A',"Someone notices the quality and care in my work — not just that it is done, but that the way it was done actually mattered to someone.",
                [new("craft_intrinsic_motivation",2)]),
            new('B',"Someone trusts me with more ownership or a harder problem — recognition through responsibility feels more real to me than words.",
                [new("autonomy_driven_motivation",2),new("autonomous_execution_preference",1)]),
            new('C',"Someone acknowledges the effort during a difficult or pressured period — being seen when things are hard means more than being praised when things are easy.",
                [new("pressure_recognition_need",2)]),
            new('D',"Someone connects my work to a bigger outcome — I find it meaningful when I can see how what I did actually moved something that mattered.",
                [new("purpose_driven_motivation",2)]),
        ]),
        new(10,4,45,"What drains you most at work?",
        [
            new('A',"Avoided conflict or indirect communication — when problems are visible but nobody names them, the silence itself becomes exhausting to work inside.",
                [new("direct_conflict_approach",2),new("risk_transparency_need",1)]),
            new('B',"Redundant process, unnecessary meetings, or overhead that does not produce anything — I can accept structure when it serves something, but process for its own sake steadily drains my motivation.",
                [new("focus_protection",2),new("visible_result_need",1)]),
            new('C',"Work that feels disconnected from any meaningful outcome — I can push through hard or boring work when I understand why it matters. When I cannot see that, the effort starts to feel hollow.",
                [new("purpose_driven_motivation",2)]),
            new('D',"Feeling underutilized or invisible — when my judgment, skills, or contribution are consistently overlooked or not sought out, it is quietly exhausting in a way that is hard to name but hard to ignore.",
                [new("autonomy_driven_motivation",2),new("proactive_feedback_need",1)]),
        ]),
        new(10,5,46,"At the end of a good workday, what most often makes it feel like time well spent?",
        [
            new('A',"I made visible progress on something that actually matters — not busy work, not preparation for future work, but real movement on something with weight.",
                [new("visible_result_need",2),new("purpose_driven_motivation",1)]),
            new('B',"I helped someone or removed something that was blocking them — knowing the day made someone else's work easier or better is enough for me.",
                [new("relational_driven_motivation",2)]),
            new('C',"I understood something I did not understand before — a day where my mental model of the problem, the system, or the people improved feels well spent even without visible output.",
                [new("learning_orientation",2)]),
            new('D',"I left things better than I found them — a cleaner codebase, a clearer process, a resolved tension. Incremental improvement in the right direction is quietly satisfying in a way that big visible progress sometimes is not.",
                [new("improvement_satisfaction",2),new("craft_intrinsic_motivation",1)]),
        ]),
    ];

    // ── Insert Questions and Options ───────────────────────────────────────

    private static void InsertQuestionsAndOptions(MigrationBuilder mb, QuestionDef[] questions)
    {
        var qRows = string.Join(",\n", questions.Select(q =>
            $"('{Q(q.Section,q.LocalQ)}','{QvId}',{EscSql(q.Text)},{q.Section},{q.OrderIndex})"));
        mb.Sql($"""
            INSERT INTO "Questions" ("Id","QuestionnaireVersionId","Text","SectionIndex","OrderIndex")
            VALUES {qRows}
            """);

        var aoRows = string.Join(",\n", questions.SelectMany(q =>
            q.Options.Select(o =>
                $"('{AO(q.Section,q.LocalQ,o.Letter)}','{Q(q.Section,q.LocalQ)}',{EscSql(o.Text)},{o.Letter - 'A' + 1})")));
        mb.Sql($"""
            INSERT INTO "AnswerOptions" ("Id","QuestionId","Text","OrderIndex")
            VALUES {aoRows}
            """);
    }

    // ── Insert DimensionWeights ────────────────────────────────────────────

    private static void InsertDimensionWeights(MigrationBuilder mb, QuestionDef[] questions)
    {
        var rows = questions.SelectMany(q =>
            q.Options.SelectMany(o =>
                o.Weights.Select(w =>
                    $"('{DW(q.Section,q.LocalQ,o.Letter,w.DimId)}','{AO(q.Section,q.LocalQ,o.Letter)}',{EscSql(w.DimId)},{w.Weight})")));

        var batch = rows.ToList();
        mb.Sql($"""
            INSERT INTO "DimensionWeights" ("Id","AnswerOptionId","DimensionId","Weight")
            VALUES {string.Join(",\n", batch)}
            """);
    }

    // ── Compute and Insert DimensionMaxScores ─────────────────────────────

    private static void InsertDimensionMaxScores(MigrationBuilder mb, QuestionDef[] questions)
    {
        // Collect all dimension IDs
        var allDims = questions
            .SelectMany(q => q.Options.SelectMany(o => o.Weights.Select(w => w.DimId)))
            .Distinct()
            .OrderBy(d => d)
            .ToList();

        var rows = new List<string>();
        foreach (var dim in allDims)
        {
            decimal maxScore = 0m;
            foreach (var q in questions)
            {
                var weightsForDim = q.Options
                    .Select(o => o.Weights.FirstOrDefault(w => w.DimId == dim)?.Weight)
                    .Where(w => w.HasValue)
                    .Select(w => w!.Value)
                    .OrderByDescending(w => w)
                    .ToList();

                if (weightsForDim.Count == 0) continue;
                var bestPrimary = (decimal)weightsForDim[0];
                var bestSecondary = weightsForDim.Count > 1 ? (decimal)weightsForDim[1] : 0m;
                maxScore += bestPrimary * 1.0m + bestSecondary * 0.5m;
            }
            if (maxScore <= 0m) maxScore = 1m; // safety guard for all-negative dims
            rows.Add($"('{DMS(dim)}','{QvId}',{EscSql(dim)},{maxScore.ToString("F4", CultureInfo.InvariantCulture)})");
        }

        mb.Sql($"""
            INSERT INTO "DimensionMaxScores" ("Id","QuestionnaireVersionId","DimensionId","MaxScore")
            VALUES {string.Join(",\n", rows)}
            """);
    }

    // ── InsightSnippets ────────────────────────────────────────────────────

    private static void InsertInsightSnippets(MigrationBuilder mb)
    {
        var snippets = new (string Dim, string Text)[]
        {
            ("upfront_clarity_need","You need goals, expectations, and the quality bar to be clear before you commit. Starting without that picture makes it hard to move with confidence."),
            ("clarity_via_written_context","You trust written records more than memory or conversation. Decisions that only exist in someone's head — or were said once in a meeting — are hard for you to rely on."),
            ("clarity_via_artifact_context","Real examples tell you more than descriptions do. You'd rather look at past code, a ticket, or an existing standard than read an explanation of how things work."),
            ("learning_by_doing","You figure things out by doing them. A small real experiment tells you more than any amount of reading or discussing upfront."),
            ("ambiguity_tolerance","You're comfortable starting before everything is settled. Not having all the answers yet doesn't stop you from moving — you'd rather begin and refine than wait for full certainty."),
            ("explicit_alignment_need","You need explicit agreement before moving forward. Acting on assumptions that nobody actually confirmed makes you uncomfortable — you'd rather check and align than assume and discover a mismatch later."),
            ("social_context_need","Context about people matters to you as much as context about the work. Understanding who does what, how the team operates, and what the dynamics are helps you make better decisions and settle in faster."),
            ("visibility_preference","You prefer work to be visible as it happens. Knowing where things are — what's moving, what's blocked, what others are working on — helps you coordinate and gives you confidence the picture is accurate."),
            ("verbal_alignment_preference","Live conversation works better for you than written exchange. You can test your understanding, catch what's unsaid, and ask the follow-up that unlocks the real point — in a way a message thread rarely can."),
            ("scheduled_communication_preference","You work better with a predictable communication rhythm. Knowing when alignment happens lets you prepare — and the consistency itself has value, even when there isn't much to discuss."),
            ("immediate_alignment_preference","When something needs addressing, you'd rather deal with it now than wait. Letting a misalignment or blocker sit until the next scheduled moment feels like unnecessary drag."),
            ("focus_protection","Uninterrupted focus time matters to you. Interruptions, unnecessary meetings, and constant context switching have a real cost — and you notice when that cost isn't justified by what they produce."),
            ("visible_result_need","You need effort to produce something you can point to. A meeting without a decision, a conversation without resolution, or a day without real progress is hard for you to justify — the output is what makes the time feel well spent."),
            ("relational_connection_need","The relationships at work matter to you independently of the output. Regular connection, familiarity, and team rhythm are worth time on their own — not everything needs to produce a deliverable to be worth doing."),
            ("iteration_preference","Fixed cycles work well for you — a predictable cadence with clear moments to plan, deliver, and reflect. The rhythm itself helps you work at your best."),
            ("flow_preference","You prefer pulling the next thing from a clear backlog and keeping work moving continuously. Waiting for the next sprint to start, or planning more than you need to begin, adds friction you'd rather not have."),
            ("upfront_planning_preference","You want the full picture before you start executing. Knowing the scope, the dependencies, and where the risks are lets you move with confidence rather than discovering surprises mid-way."),
            ("adaptive_planning_preference","You'd rather start with a rough direction and refine as you learn than commit to a full plan before you understand the problem. Too much upfront planning creates false certainty that gets in the way."),
            ("evidence_based_planning_preference","When something is uncertain or risky, you'd rather run a small experiment than commit to a plan built on untested assumptions. What you learn from a real test is worth more than what you can reason from the outside."),
            ("participatory_decision_preference","When a decision affects your work, you expect to be part of it before it's made — not told about it afterward. Being informed after the fact, when the direction is already set, doesn't feel like involvement."),
            ("planning_boundary_protection","You need the plan to be protected from constant interruption. Collecting changes and handling them at the next planning point — rather than mid-flow — is how you stay productive and make commitments that mean something."),
            ("capacity_protection","If something new is added, something else needs to come off. You expect that trade-off to be made explicitly — piling work on top of existing commitments without acknowledging the cost isn't acceptable to you."),
            ("sustainable_pace_preference","Sustainable pace matters to you. You resist scope inflation and overcommitment — not because you're averse to hard work, but because you know that burning out helps nobody."),
            ("tradeoff_visibility","When a decision is made, you want to know what was considered and what's being given up. A decision without visible trade-offs feels incomplete — you need to understand the cost, not just the outcome."),
            ("change_tolerance","You can absorb changes to scope or direction without much friction. When something important comes up, you'd rather respond to it than protect the original plan for its own sake."),
            ("ownership_boundary_clarity","You need to know who owns what. Unclear boundaries around decisions and responsibilities create the conditions for things to be dropped or duplicated — and for confusion when something needs resolving."),
            ("autonomous_pace_preference","You manage your own tempo. Repeated check-ins that don't change anything or help the work feel like monitoring rather than support — and that erodes trust over time."),
            ("autonomous_execution_preference","Once you own something, you expect to decide how to do it. Detailed direction on work that's already yours signals that your judgment isn't trusted — and that makes ownership feel hollow."),
            ("support_over_control_preference","You can work well with oversight when it genuinely supports you. What you can't work with is oversight driven by anxiety or a need to approve every step — that's control, not support, and the difference is felt."),
            ("unblocking_support_need","You need to be able to get help when you're stuck. Not being able to reach guidance on important decisions — or having that support be consistently unavailable — leaves you feeling unsupported in a way that undermines ownership."),
            ("proactive_feedback_need","You need more than silence broken only by problems. Ongoing signals — calibration, encouragement, genuine exchange — help you stay oriented and engaged. Working without that feedback loop starts to feel like working in isolation."),
            ("directive_support_preference","When you're stuck, you want a concrete answer or recommendation — not a facilitated exploration of the options. The value of support in that moment is getting unblocked, not having someone think through it with you."),
            ("help_seeking_safety_need","You need to be able to ask for help without it counting against you. If seeking support signals weakness or incompetence, people stop asking — and that's when problems grow quietly until they're too big to miss."),
            ("autonomous_problem_resolution","Your instinct is to resolve problems yourself before bringing others in. Involving people too early creates noise and dependency — and most things can be resolved without escalating if you give them a fair attempt first."),
            ("risk_transparency_need","You'd rather raise a risk early than let it become a surprise. Problems held back until they're urgent are harder to deal with — and you hold yourself to the same standard you expect from others."),
            ("pragmatic_completion_preference","You consider work done when the core problem is solved reliably. Follow-up items and edge cases matter, but they shouldn't always block a handoff — steady progress beats waiting for perfect."),
            ("external_validation_need","Another set of eyes makes the work feel solid. You trust a handoff more when someone else has reviewed it — not because you doubt your own judgment, but because a second perspective is how you catch what you can't see yourself."),
            ("impact_calibration_preference","Your instinct when something goes wrong is to assess before reacting. Understanding the actual severity and impact — not just the immediate alarm — leads to more proportionate, useful responses."),
            ("root_cause_orientation","A fix that doesn't address the cause isn't really a fix. You want to understand why something happened — the pattern, the underlying condition — so the same problem doesn't come back."),
            ("quality_protection_preference","You protect the quality bar even when deadlines push back. Some risks aren't worth accepting regardless of the pressure — and you're willing to push back on scope or timeline rather than ship something that compromises what matters."),
            ("accountability_preference","When something goes wrong, your instinct is to own it and fix it. Explaining why it happened or questioning whether it's your problem comes second — repair comes first."),
            ("critical_feedback_validation","You don't treat every piece of critical feedback as a problem by default. Before accepting a concern, you examine whether it actually reflects a real gap — the concern itself can be worth questioning."),
            ("feedback_content_primacy_giving","When you give feedback, what matters most is that the concern is accurate and useful — not how comfortable it is to receive. A valid point shouldn't be dismissed just because it was hard to hear."),
            ("feedback_delivery_awareness_giving","Feedback only works if the other person can actually use it. When you give feedback, how you deliver it is as important as what you're saying — getting the substance right means nothing if the delivery makes it impossible to hear."),
            ("feedback_content_primacy_receiving","When you receive feedback, you focus on the substance — even if the delivery was clumsy or harsh. You don't let style get in the way of hearing what might be a valid point."),
            ("feedback_delivery_awareness_receiving","How feedback is delivered affects whether you can actually process it. If the delivery is inappropriate or unclear, engaging with the concern becomes genuinely harder — it's not a preference, it's how you work."),
            ("communication_norm_alignment","When a feedback or communication pattern creates friction, your instinct is to agree on how things should work going forward — not just resolve the immediate incident and move on. The pattern itself is what needs fixing."),
            ("private_feedback_preference","Your default is to raise concerns privately. Bringing a problem into a public or group context before trying to resolve it one-on-one feels like a last resort — and often the wrong first move."),
            ("dignity_in_feedback_need","If feedback is raised in front of others, how it's handled matters enormously to you. Being criticised or embarrassed publicly makes it hard to engage with the concern itself — dignity is a precondition, not a preference."),
            ("relational_repair_need","After a difficult exchange, you need the relationship to be repaired separately from the issue being resolved. A follow-up conversation — acknowledging what happened and resetting — is what lets you move forward without it hanging over everything."),
            ("calm_urgency_preference","When things are urgent, you need the communication to be calm and clear — not dramatic. Performed stress or anxiety doesn't help you move faster; an unambiguous statement of what matters most does."),
            ("pressure_driven_motivation","High stakes and direct accountability actually sharpen your focus. When the pressure is real and communicated as such, you respond — that kind of urgency is a driver for you, not just noise."),
            ("positive_motivation_preference","Encouragement and purpose work better than pressure for you. When someone frames the work as exciting or meaningful and makes clear they're behind you, that's what moves you — not consequences or urgency."),
            ("autonomous_motivation","You don't need external motivation. Pep talks and deadline speeches mostly get in the way — you already know what needs doing and you drive yourself. What you need is clarity and space, not a push."),
            ("pressure_recognition_need","Recognition matters most to you when things are hard. Being seen during a pressured or difficult stretch means more than routine praise when everything is going smoothly — that's when acknowledgment actually lands."),
            ("direct_conflict_approach","When there's friction, you address it directly with the person — even when it's uncomfortable. Naming it early, before it has time to harden, is better than working around it or waiting for it to resolve itself."),
            ("communication_friction_tolerance","You can absorb uncomfortable communication styles without needing to address them every time. Feeling personally bothered isn't enough reason to escalate — you let things go unless they're genuinely affecting something that matters."),
            ("team_impact_orientation","Your threshold for acting on friction is team impact, not personal discomfort. How something makes you feel isn't the measure — what matters is whether it's affecting the team or the work."),
            ("conflict_avoidance_tendency","Direct confrontation of uncomfortable patterns isn't your first instinct. You need time and distance to process before you can decide whether and how to act — engaging before you've had that space usually doesn't go well."),
            ("indirect_conflict_processing","When something is bothering you interpersonally, your instinct is to talk it through with someone you trust — not raise it directly with the person involved, at least not first. Naming it out loud helps you figure out what you actually think."),
            ("tension_deescalation_preference","You believe tense conversations rarely go well while the tension is still running high. Giving both people time to settle produces better conversations — engaging while it's still active tends to produce reactions, not resolution."),
            ("issue_escalation_preference","Escalation is a legitimate tool to you, not a last resort. When a problem exceeds what the people involved can resolve — because of scope, authority, or a repeated pattern — bringing someone with more authority in is the right call, not a failure."),
            ("mutual_understanding_need","In difficult conversations, what matters most to you is that both perspectives are genuinely heard — not just that a decision gets made. You need acknowledgment before you can move on, even if no clear agreement is reached."),
            ("shared_accountability_preference","Difficult conversations feel more honest when both people own something. One person diagnosing the problem while the other takes all the responsibility doesn't sit right with you — real accountability is usually shared."),
            ("facilitation_openness","You see structured facilitation as a real option for resolving conflict — not a sign that things have gone badly wrong. Whether you're participating or leading, bringing a third party in to structure the conversation can be the most effective path forward."),
            ("proactive_conflict_culture_preference","You believe conflict is easier to handle when teams build habits for it before something breaks. Retrospectives that include how people are doing, norms for raising concerns, regular check-ins where tension can be named — these are what make a team safe to be honest in."),
            ("craft_intrinsic_motivation","The work itself is what drives you. Building something well, solving a hard problem, getting the details right — that's satisfying in its own right, independent of whether anyone notices or what it's ultimately for."),
            ("purpose_driven_motivation","You need to understand why what you're doing matters. When the purpose is clear and the work connects to something meaningful, you're engaged. When you can't see that connection, motivation is genuinely harder to sustain."),
            ("autonomy_driven_motivation","Real ownership is what makes work meaningful to you. Being trusted with a problem and given the freedom to solve it your way is energizing. Without that, even interesting work starts to feel hollow."),
            ("relational_driven_motivation","The people are what make work worth doing for you. The team, the shared experience of building something together, the relationships that form in the process — these matter more as a source of energy than the work itself."),
            ("learning_orientation","A day where you understand something better feels well spent — even without visible output. Improved mental models, new insight, a clearer picture of the system or the people: these are worth a day on their own."),
            ("improvement_satisfaction","You find quiet satisfaction in leaving things better than you found them. A cleaner codebase, a clearer process, a resolved tension — small steady improvement in the right direction is enough, without needing a big visible result or recognition."),
            ("delivery_reflection_priority","You think the most important thing a team can examine is whether it can execute and deliver reliably. Process, planning, and delivery flow deserve dedicated reflection time — operational health is the foundation everything else rests on."),
            ("technical_reflection_priority","You think technical quality deserves a seat at the table when a team reflects. Incidents, maintainability, and soundness aren't secondary to delivery speed — they're what determines whether the team can keep delivering at all."),
            ("interpersonal_reflection_priority","You think collaboration patterns and communication habits are worth examining — even when they feel less urgent than delivery. Left unexamined, friction here tends to show up in the work eventually."),
            ("safety_reflection_priority","You think how people are treated and whether the team feels safe to be honest are foundational topics for reflection. Problems hidden by an unsafe culture are the hardest to fix — by the time they surface, they've usually already done damage."),
        };

        var rows = string.Join(",\n", snippets.Select(s =>
            $"('{IS(s.Dim)}',{EscSql(s.Dim)},{EscSql(s.Text)})"));
        mb.Sql($"""
            INSERT INTO "InsightSnippets" ("Id","DimensionId","Text")
            VALUES {rows}
            """);
    }

    // ── DimensionGroups ────────────────────────────────────────────────────

    private static void InsertDimensionGroups(MigrationBuilder mb)
    {
        var groups = new (string Id, string Title, int Order)[]
        {
            ("work_context_expectations_and_alignment","Work context, expectations, and alignment",1),
            ("how_you_use_live_collaboration","How you use live collaboration",2),
            ("how_you_plan_and_handle_change","How you plan and handle change",3),
            ("how_you_hold_ownership_and_use_support","How you hold ownership and use support",4),
            ("how_you_approach_quality_and_risk","How you approach quality and risk",5),
            ("how_you_handle_feedback","How you handle feedback",6),
            ("how_you_respond_to_pressure_and_urgency","How you respond to pressure and urgency",7),
            ("how_you_handle_conflict_and_tension","How you handle conflict and tension",8),
            ("what_gives_you_energy_and_meaning","What gives you energy and meaning",9),
            ("what_you_think_teams_should_reflect_on","What you think teams should reflect on",10),
        };

        var rows = string.Join(",\n", groups.Select(g =>
            $"('{DG(g.Id)}',{EscSql(g.Id)},{EscSql(g.Title)},{g.Order})"));
        mb.Sql($"""
            INSERT INTO "DimensionGroups" ("Id","GroupId","Title","OrderIndex")
            VALUES {rows}
            """);
    }

    // ── DimensionGroupMemberships ──────────────────────────────────────────

    private static void InsertDimensionGroupMemberships(MigrationBuilder mb)
    {
        var memberships = new (string Group, string[] Dims)[]
        {
            ("work_context_expectations_and_alignment", ["upfront_clarity_need","clarity_via_written_context","clarity_via_artifact_context","learning_by_doing","ambiguity_tolerance","explicit_alignment_need","social_context_need","visibility_preference"]),
            ("how_you_use_live_collaboration", ["verbal_alignment_preference","scheduled_communication_preference","immediate_alignment_preference","focus_protection","visible_result_need","relational_connection_need"]),
            ("how_you_plan_and_handle_change", ["iteration_preference","flow_preference","upfront_planning_preference","adaptive_planning_preference","evidence_based_planning_preference","participatory_decision_preference","planning_boundary_protection","capacity_protection","sustainable_pace_preference","tradeoff_visibility","change_tolerance"]),
            ("how_you_hold_ownership_and_use_support", ["ownership_boundary_clarity","autonomous_pace_preference","autonomous_execution_preference","support_over_control_preference","unblocking_support_need","proactive_feedback_need","directive_support_preference","help_seeking_safety_need","autonomous_problem_resolution"]),
            ("how_you_approach_quality_and_risk", ["risk_transparency_need","pragmatic_completion_preference","external_validation_need","impact_calibration_preference","root_cause_orientation","quality_protection_preference","accountability_preference","critical_feedback_validation"]),
            ("how_you_handle_feedback", ["feedback_content_primacy_giving","feedback_delivery_awareness_giving","feedback_content_primacy_receiving","feedback_delivery_awareness_receiving","communication_norm_alignment","private_feedback_preference","dignity_in_feedback_need","relational_repair_need"]),
            ("how_you_respond_to_pressure_and_urgency", ["calm_urgency_preference","pressure_driven_motivation","positive_motivation_preference","autonomous_motivation","pressure_recognition_need"]),
            ("how_you_handle_conflict_and_tension", ["direct_conflict_approach","communication_friction_tolerance","team_impact_orientation","conflict_avoidance_tendency","indirect_conflict_processing","tension_deescalation_preference","issue_escalation_preference","mutual_understanding_need","shared_accountability_preference","facilitation_openness","proactive_conflict_culture_preference"]),
            ("what_gives_you_energy_and_meaning", ["craft_intrinsic_motivation","purpose_driven_motivation","autonomy_driven_motivation","relational_driven_motivation","learning_orientation","improvement_satisfaction"]),
            ("what_you_think_teams_should_reflect_on", ["delivery_reflection_priority","technical_reflection_priority","interpersonal_reflection_priority","safety_reflection_priority"]),
        };

        var rows = new List<string>();
        foreach (var (group, dims) in memberships)
        {
            for (var i = 0; i < dims.Length; i++)
            {
                rows.Add($"('{DGM(group,dims[i])}','{DG(group)}',{EscSql(dims[i])},{i+1})");
            }
        }

        mb.Sql($"""
            INSERT INTO "DimensionGroupMemberships" ("Id","DimensionGroupId","DimensionId","OrderIndex")
            VALUES {string.Join(",\n", rows)}
            """);
    }

    // ── SQL helpers ────────────────────────────────────────────────────────

    private static string EscSql(string s) => $"'{s.Replace("'", "''")}'";
}
