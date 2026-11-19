using System;
using System.Collections.Generic;

namespace KleeneStar.Help.WebSeed
{
    /// <summary>
    /// The content of the help pages.
    /// </summary>
    public static partial class HelpSeeder
    {
        /// <summary>
        /// The reserved landing label of the compact how-to pages.
        /// </summary>
        /// <remarks>
        /// The start page reads the labels through <c>KleeneStar.Core.WebFragment.Landing.LandingLabel</c>;
        /// the strings are repeated here because a label is data, matched without case.
        /// </remarks>
        public const string LabelHelp = "Help";

        /// <summary>
        /// The reserved landing label of the frequently asked questions.
        /// </summary>
        public const string LabelFaq = "FAQ";

        /// <summary>
        /// The reserved landing label of the first steps.
        /// </summary>
        public const string LabelFirstSteps = "First Steps";

        private static readonly Guid HomeId = Guid.Parse("5F3348C0-AC24-4E66-B145-713C2F83253E");
        private static readonly Guid FirstStepsId = Guid.Parse("1E4CB983-C18D-4598-85A9-B333948D106C");
        private static readonly Guid GuidesId = Guid.Parse("761AFD9B-AD94-4847-B758-B379BCA950A0");
        private static readonly Guid FaqId = Guid.Parse("83BD43FF-3319-424A-A0C9-A7F14F5F409A");

        /// <summary>
        /// The pages in seeding order: a chapter is written before its pages.
        /// </summary>
        private static readonly IReadOnlyList<HelpPage> _pages =
        [
            // ------------------------------------------------------------------ home page
            new
            (
                HomeId, 1, HelpPageRole.Home, null, null,
                "KleeneStar Help",
                "home",
                "Welcome to the help of KleeneStar. Here you find the first steps for a new colleague, guides for the everyday work and the answers to the questions asked most often. Every page is an ordinary document of this installation: it is searchable, versioned and written by the people who know the answer.",
                [
                    new
                    (
                        "Start here",
                        new HelpParagraph("The help is organized in three chapters. Begin with the first steps if KleeneStar is new to you; come back to the guides when a task is new; look into the questions when something surprises you."),
                        new HelpPoints
                        (
                            "[[HELP-2]] The four ideas KleeneStar is built from and your first issue.",
                            "[[HELP-7]] Step by step through the tasks of the everyday work.",
                            "[[HELP-16]] Short answers to the questions asked most often."
                        )
                    ),
                    new
                    (
                        "All help pages",
                        new HelpParagraph("The list below is read live from this workspace, so a page written today appears here today."),
                        new HelpPageList()
                    ),
                    new
                    (
                        "Where else help is found",
                        new HelpPoints
                        (
                            "The **help menu** in the header lists the guides, the questions and the first steps from every page.",
                            "The **start page** shows the same pages in its help area.",
                            "The **search** in the header finds every help page by its words, like any other document."
                        )
                    ),
                    new
                    (
                        "Write help of your own",
                        new HelpParagraph("The administrators of this workspace keep the help. A new page is a document of the class **Help page**; it keeps the order of the template: lead with illustration, horizontal rule, table of contents and one heading 1 per topic."),
                        new HelpSteps
                        (
                            "Open the chapter the page belongs to and create the page from there, so it lands below the chapter.",
                            "Write the page in the editor. The instruction text at the top names the template version and is shown while editing only.",
                            "Give the page the label `Help`, `FAQ` or `First Steps` to show it on the start page and in the help menu."
                        ),
                        new HelpNotice("warning", "Archive instead of delete", "The pages that come with KleeneStar return on the next start when they are deleted. Archive a page you do not want; an archived page leaves the start page and the help menu by itself.")
                    )
                ]
            ),

            // ------------------------------------------------------------------ first steps
            new
            (
                FirstStepsId, 2, HelpPageRole.Chapter, null, null,
                "First Steps",
                "first-steps",
                "Your first day with KleeneStar. These pages explain the few ideas everything else is built from and walk you through your first issue. Read them in order; each takes a few minutes.",
                [
                    new
                    (
                        "In this chapter",
                        new HelpChildren()
                    ),
                    new
                    (
                        "After the first steps",
                        new HelpParagraph("Continue with the guides in [[HELP-7]], or look up a question in [[HELP-16]].")
                    )
                ]
            ),
            new
            (
                Guid.Parse("1796AE7E-83F4-4006-B5E6-0DA5E25EA967"), 3, HelpPageRole.Topic, FirstStepsId, LabelFirstSteps,
                "Your first issue in three steps",
                "first-issue",
                "Pick a workspace, press create, and describe what happened in one sentence. That is enough to start; the rest is filled in as the issue moves.",
                [
                    new
                    (
                        "The three steps",
                        new HelpSteps
                        (
                            "Press **Create** in the header. The dialog asks for the workspace first: choose the team that should handle the issue.",
                            "Choose the class, for example an incident or a task, and a template if one fits your case.",
                            "Write the summary as one sentence that says what happened. Press **Create issue**: the new issue opens at once."
                        )
                    ),
                    new
                    (
                        "What a good summary says",
                        new HelpPoints
                        (
                            "What is affected: the service, the device or the document.",
                            "What happens instead of what should happen.",
                            "Since when, if it is not right now."
                        ),
                        new HelpNotice("info", "Tip", "Describe the details in the description below the summary. A colleague reading the list sees the summary only, so it should stand on its own.")
                    ),
                    new
                    (
                        "What happens next",
                        new HelpParagraph("The issue starts in the first state of its workflow. The team sees it in its overview, takes it over and moves it on. You find it again under **My issues** and are told when somebody comments on it.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("ED3CA040-E9DC-483B-9E74-6DD92224943C"), 4, HelpPageRole.Topic, FirstStepsId, LabelFirstSteps,
                "Understand templates",
                "templates-understand",
                "A template is a pre-filled form, not a separate kind of issue. What it fills in can be changed before saving.",
                [
                    new
                    (
                        "What a template is",
                        new HelpParagraph("Every template belongs to one class. It names values for the fields of that class, and often a summary and a description to start from. When you pick it while creating, those values are already in the form."),
                        new HelpPoints
                        (
                            "A template saves typing for the cases that come up again and again.",
                            "It makes similar issues look alike, so they are easier to compare.",
                            "Some templates create a whole structure: an order together with its sub-tasks."
                        )
                    ),
                    new
                    (
                        "Your answer wins",
                        new HelpParagraph("Whatever you enter replaces what the template proposed. A field you leave empty keeps the value of the template.")
                    ),
                    new
                    (
                        "Learn more",
                        new HelpParagraph("The guide [[HELP-9]] shows how to choose and use a template step by step.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("2DEB3683-7A5C-44C5-89F0-C8792125B2CE"), 5, HelpPageRole.Topic, FirstStepsId, LabelFirstSteps,
                "The basic functions at a glance",
                "basics",
                "Reading an issue, changing it, commenting on it, and following it. Everything else in KleeneStar is built from those four.",
                [
                    new
                    (
                        "Read",
                        new HelpParagraph("An issue page shows the summary and description, the details of the class, the people involved, the dates and the history. The preview pane of a list shows the same without leaving the list.")
                    ),
                    new
                    (
                        "Change",
                        new HelpParagraph("Click a value in the details to change just that value, or open the edit dialog for the whole record. A workflow transition moves the issue to its next state.")
                    ),
                    new
                    (
                        "Comment",
                        new HelpParagraph("The conversation sits at the foot of every page. Write a comment, reply to one, or react to it. See [[HELP-12]].")
                    ),
                    new
                    (
                        "Follow",
                        new HelpParagraph("Watch an issue to be told about its changes. The bell in the header collects what happened to the records you follow.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("7A7BE4D6-76BC-43FA-9577-24A7F125F58D"), 6, HelpPageRole.Topic, FirstStepsId, LabelFirstSteps,
                "Class, view, workspace",
                "structure",
                "A workspace holds the work of a team. A class says which fields an issue has. A view is one way of looking at a set of issues: a list, a board, a calendar.",
                [
                    new
                    (
                        "Workspace",
                        new HelpParagraph("A workspace belongs to a team or a topic, for example the service desk or the development. It has its own key, which every record in it carries: `SD-12` is record 12 of the workspace `SD`. Who may read and write in a workspace is set by its permissions.")
                    ),
                    new
                    (
                        "Class",
                        new HelpParagraph("A class describes one kind of record: its fields, its forms, its workflow and its templates. A class is of one object type:"),
                        new HelpPoints
                        (
                            "**Issues**: work that moves through states, like incidents or tasks.",
                            "**Assets**: things that are kept, like devices or contracts.",
                            "**Documents**: pages in a page tree, like this one.",
                            "**Blog entries**: news on a timeline."
                        )
                    ),
                    new
                    (
                        "View",
                        new HelpParagraph("The overview of a workspace shows its records in tabs: a table, a list, a Kanban board, a dashboard, a Gantt chart or a calendar. A view changes how you look at the records, never the records themselves. Insights do the same across several workspaces.")
                    )
                ]
            ),

            // ------------------------------------------------------------------ guides
            new
            (
                GuidesId, 7, HelpPageRole.Chapter, null, null,
                "Guides",
                "guides",
                "Step by step through the tasks of the everyday work: creating and handing over issues, templates, saved views, comments, documents, search and the classification of records.",
                [
                    new
                    (
                        "In this chapter",
                        new HelpChildren()
                    ),
                    new
                    (
                        "How the guides are written",
                        new HelpParagraph("Each guide starts with what the task is for, then lists the steps, then the details worth knowing. The table of contents at the top of a guide jumps to every section.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("821C8B40-7D55-42FF-AA70-802EC9C79713"), 8, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Create an issue",
                "create-issue",
                "Use the create button in the header, pick the class the issue belongs to, and fill in the summary. Everything else can follow later; the workflow asks for what it needs when it needs it.",
                [
                    new
                    (
                        "Open the dialog",
                        new HelpParagraph("The **Create** button sits in the header of every page once you are signed in. It opens a dialog that leads you through a few steps; a step that has nothing to ask is skipped.")
                    ),
                    new
                    (
                        "Steps of the dialog",
                        new HelpSteps
                        (
                            "**Workspace**: the team the issue is meant for.",
                            "**Class**: the kind of issue. Only the classes you may write in are offered.",
                            "**Template**: a pre-filled starting point, or none.",
                            "**Summary and description**: what happened, in one sentence and then in detail.",
                            "**Values**: the fields of the class, for example priority or category.",
                            "**Security level**: who may see the issue. The default of the class is preselected."
                        )
                    ),
                    new
                    (
                        "After saving",
                        new HelpParagraph("The new issue opens at once and carries its key. Created from an open document, a new document is placed below that document in the page tree."),
                        new HelpNotice("info", "Tip", "Going back in the dialog keeps everything you entered; a later step is rebuilt from your answers.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("E98425C4-8397-49AC-927B-64C039D97645"), 9, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Work with templates",
                "templates-work",
                "A template pre-fills a form so only what is specific to your case is left to write. Templates belong to a class and are offered when you create an issue of that class.",
                [
                    new
                    (
                        "Choose a template",
                        new HelpSteps
                        (
                            "Start creating as usual and choose the workspace and the class.",
                            "The next step shows the templates of the class as tiles. Pick the one that fits, or continue without one.",
                            "The following steps are pre-filled. Change what differs in your case and finish."
                        )
                    ),
                    new
                    (
                        "What a template fills in",
                        new HelpPoints
                        (
                            "Values of fields, for example a category or a priority.",
                            "The summary and the description the record starts with.",
                            "For a composite template: a sub-task per part, related to the order and placed in the workspaces that do the work."
                        )
                    ),
                    new
                    (
                        "Keep templates useful",
                        new HelpParagraph("Templates are kept by the administrators of a class. A template whose values no longer match the fields of its class fills in less than it promises, so tell them when a template looks outdated.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("21E929D8-CEF3-4B3E-9B8A-D4D4F4E3B947"), 10, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Assign and hand over an issue",
                "assign",
                "Set the assignee on the people card of the issue. Handing over means changing that person; the history keeps who worked on it, and the new assignee finds it under My issues.",
                [
                    new
                    (
                        "Take an issue over",
                        new HelpParagraph("Open the issue and press **Assign to me** on the people card. The card shows the assignee, the creator, the approvers and the watchers of the issue.")
                    ),
                    new
                    (
                        "Hand an issue over",
                        new HelpSteps
                        (
                            "Write a comment for the next person: what is done, what is open, what to watch out for.",
                            "Change the assignee to the colleague taking over.",
                            "Stay a watcher if you want to know how it ends."
                        )
                    ),
                    new
                    (
                        "Where assigned issues appear",
                        new HelpParagraph("**My issues** in the sidebar of the start page lists the issues assigned to you and the ones you raised. The start page shows your open ones under **My work**. **Unassign** gives an issue back to the team.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("F781D0EB-1584-4B0A-AEC7-5D3702A0D576"), 11, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Filter and save a view",
                "filter",
                "Narrow a list with the quick filters or the search field, then save the result as a saved search. A saved view belongs to you until you share it.",
                [
                    new
                    (
                        "Narrow a list",
                        new HelpPoints
                        (
                            "The **quick filters** above a list switch common conditions on and off with one click.",
                            "The **search field** finds records by their words.",
                            "The **advanced search** takes a WQL query for exact conditions, see [[HELP-14]]."
                        )
                    ),
                    new
                    (
                        "Save the result",
                        new HelpSteps
                        (
                            "Open **Search** in the header and enter the query.",
                            "Press **Save search** and give it a short, recognizable name.",
                            "Arrange the columns of the result table; they are kept with the saved search."
                        ),
                        new HelpParagraph("**Save as new search** keeps the original and stores a copy. A starred search is pinned to the top of the sidebar.")
                    ),
                    new
                    (
                        "Share a saved search",
                        new HelpParagraph("A saved search nobody shared is private. Its **Permissions** dialog decides who else may run, change or administer it. Colleagues find it under **Shared with me** in the search menu.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("7E7642A4-27BA-4653-AC32-165884E135E0"), 12, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Comments and attachments",
                "comments",
                "Every issue carries a comment thread and a list of attachments. Both are part of the record and visible to everybody who can see it.",
                [
                    new
                    (
                        "Comment",
                        new HelpParagraph("The conversation is docked at the foot of every page: the thread above, the composer below. Write, reply, react, or pin the comment that matters most. The badge counts the comments and replies.")
                    ),
                    new
                    (
                        "Internal notes",
                        new HelpParagraph("In a service desk the composer offers an internal note beside the public comment. An internal note is read by the service team only; a customer, the requester included, never sees it."),
                        new HelpNotice("warning", "Check the audience", "Before you send, look at the audience the composer shows. A public comment is read by everybody who can see the record.")
                    ),
                    new
                    (
                        "Attach files",
                        new HelpParagraph("Drop a file anywhere on the page, or use the attachment card. Each attachment keeps its description and can be downloaded by everybody who can see the record.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("415690C6-CFB5-4C8B-A2CD-BECADBAAFC5D"), 13, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Write and publish a document",
                "document",
                "Documents and blog entries are written in the editor and published when they are ready. Until then the readers keep seeing the last published text.",
                [
                    new
                    (
                        "Drafts save themselves",
                        new HelpParagraph("Every change in the editor is stored as a draft at once. Closing the editor keeps the draft; opening it again continues where you left off. A draft is shared, so a second author sees and continues the same text.")
                    ),
                    new
                    (
                        "Publish",
                        new HelpSteps
                        (
                            "Open the document and press the edit button.",
                            "Write; the save indicator tells you when the draft is stored.",
                            "Press **Publish**. The text becomes the new version and is recorded in the history."
                        )
                    ),
                    new
                    (
                        "Build richer pages",
                        new HelpParagraph("Type `{{` or use the puzzle button to insert an add-on: a reference to an object, a live list or key figure, the child pages, a table of contents, the latest news or a notice box. An add-on shows current data every time the page is opened."),
                        new HelpNotice("info", "Instruction texts", "An instruction text addresses the authors of a page. It is shown in the editor and left out of the reading view, which is how the template of these help pages names its version.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("F0260956-91DD-480B-BBD2-05CD7A59B23F"), 14, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Find anything with search and WQL",
                "search",
                "The search in the header finds records across all workspaces. For exact conditions write a WQL query: a short sentence of attributes, operators and values.",
                [
                    new
                    (
                        "Quick search",
                        new HelpParagraph("Type into **Search all workspaces** in the header. The matches appear while you type, together with the records you opened recently. Only what you may read is found.")
                    ),
                    new
                    (
                        "Write a WQL query",
                        new HelpPoints
                        (
                            "`Summary ~ \"login\"` finds records whose summary contains the word.",
                            "`Key = \"SD-12\"` finds one record by its key.",
                            "`Summary ~ \"printer\" and not Summary ~ \"toner\"` combines conditions with `and`, `or`, `not` and brackets.",
                            "`within(\"HELP-7\")` selects a page and everything below it in the page tree.",
                            "`order by Summary desc take 10` sorts and limits the result."
                        )
                    ),
                    new
                    (
                        "Keep a query",
                        new HelpParagraph("The advanced search remembers your recent queries. A query you need again is better kept as a saved search, see [[HELP-11]].")
                    )
                ]
            ),
            new
            (
                Guid.Parse("80C94596-7FE6-4491-B235-536D43EEBCD1"), 15, HelpPageRole.Topic, GuidesId, LabelHelp,
                "Classify a record with a security level",
                "security",
                "A security level narrows who may see a record. A record without a level is visible to everybody who may read its workspace; a record with one only to the groups the level names.",
                [
                    new
                    (
                        "How a level works",
                        new HelpPoints
                        (
                            "The levels are defined per class by its administrators, each naming the groups cleared for it.",
                            "A level that names no group is closed: nobody sees a record carrying it.",
                            "Lists, searches and counts leave out what you are not cleared for."
                        )
                    ),
                    new
                    (
                        "Classify a record",
                        new HelpSteps
                        (
                            "Open the record and choose **Security level** in the overflow menu.",
                            "Pick a level you are cleared for, or **unclassified**.",
                            "Save. The change is recorded in the history of the record."
                        ),
                        new HelpParagraph("When creating a record, the last step of the dialog offers the same choice with the default of the class preselected.")
                    ),
                    new
                    (
                        "Personal data",
                        new HelpNotice("warning", "Classify early", "Give a record that holds personal or confidential information its level when you create it, not afterwards.")
                    )
                ]
            ),

            // ------------------------------------------------------------------ questions
            new
            (
                FaqId, 16, HelpPageRole.Chapter, null, null,
                "Frequently asked questions",
                "faq",
                "Short answers to the questions asked most often. Each answer is a page of its own, so it can be found, linked and improved on its own.",
                [
                    new
                    (
                        "In this chapter",
                        new HelpChildren()
                    ),
                    new
                    (
                        "Your question is missing",
                        new HelpParagraph("Ask the administrators of your installation. A question asked twice deserves a page in this chapter.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("40BF658E-496D-4F0D-94DC-E227170D4E98"), 17, HelpPageRole.Topic, FaqId, LabelFaq,
                "Who can see my issue?",
                "visibility",
                "Everybody who may read the workspace the issue lives in. A security level on the issue narrows that to the groups the level names.",
                [
                    new
                    (
                        "The workspace decides first",
                        new HelpParagraph("The permissions of a workspace say who may read it, who may write in it and who administers it. A workspace nobody has set permissions on is open to everybody.")
                    ),
                    new
                    (
                        "The security level narrows",
                        new HelpParagraph("A classified issue is visible only to the members of the groups its level names. See [[HELP-15]].")
                    ),
                    new
                    (
                        "Customers in the portal",
                        new HelpParagraph("A customer sees in the portal the requests they raised or take part in, and those of their organization that are not restricted. Internal notes never reach the portal.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("9153C656-5122-4DA5-B3A2-F7B17B49418A"), 18, HelpPageRole.Topic, FaqId, LabelFaq,
                "What happens when a deadline passes?",
                "deadline",
                "The service level clock of the issue shows the breach. What follows is decided by the rules of the workspace; nothing is deleted or closed by the clock itself.",
                [
                    new
                    (
                        "The clock",
                        new HelpParagraph("An issue held to a service level agreement shows its clocks on the issue page: how much time is left, paused while the issue waits for the customer, and breached once the target has passed.")
                    ),
                    new
                    (
                        "Before and after the breach",
                        new HelpPoints
                        (
                            "Shortly before the target the issue is reported as at risk.",
                            "At the target it is reported as breached.",
                            "Automation rules of the workspace can react to both, for example by raising the priority or informing a lead."
                        )
                    ),
                    new
                    (
                        "In the reports",
                        new HelpParagraph("The service level compliance report of an insight shows how many issues met their targets.")
                    )
                ]
            ),
            new
            (
                Guid.Parse("D926F608-02A4-43D5-869F-23212131B967"), 19, HelpPageRole.Topic, FaqId, LabelFaq,
                "Can I replace this start page?",
                "start-page",
                "Yes. Choose start page in the greeting of the start page leads to the insights; an insight shows exactly the records you want to see first.",
                [
                    new
                    (
                        "What the start page offers",
                        new HelpParagraph("The start page shows your key figures, your open work, the news, the pinned pages, the help and the latest activity. Its figures and lists are your own, and it shows only what you may read.")
                    ),
                    new
                    (
                        "Your own overview",
                        new HelpSteps
                        (
                            "Press **Choose start page** in the greeting, or open **Insights** in the header.",
                            "Create an insight and describe its records with a WQL query, for example the open issues of your team.",
                            "Add the tabs you work in: a table, a Kanban board, a calendar or the reports."
                        )
                    )
                ]
            ),
            new
            (
                Guid.Parse("B33E8A7D-4A45-4D34-8C9A-3968536AB288"), 20, HelpPageRole.Topic, FaqId, LabelFaq,
                "How do I find an old issue?",
                "find-old",
                "Search across all workspaces from the header, or open the issue list and reset the quick filters. A closed issue leaves the default filters but stays in the list.",
                [
                    new
                    (
                        "Search",
                        new HelpParagraph("The search in the header looks into every workspace you may read, closed issues included. Recently opened records are offered before you type.")
                    ),
                    new
                    (
                        "The list of the workspace",
                        new HelpParagraph("The default quick filters of an overview often hide what is done. Switch them off, or search the list for words of the summary.")
                    ),
                    new
                    (
                        "An issue that moved",
                        new HelpParagraph("An issue moved to another workspace gets a new key, and its old key keeps working: opening it leads to the issue under its new key.")
                    )
                ]
            )
        ];
    }
}
