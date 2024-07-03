CREATE TABLE [cost_currency_category] (
    [id] int NOT NULL IDENTITY,
    [code] char(3) NOT NULL,
    [name] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__cost_cur__3213E83F29BE02DC] PRIMARY KEY ([id])
);
GO


CREATE TABLE [favor_category] (
    [id] int NOT NULL IDENTITY,
    [category] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__favor_ca__3213E83FFD50B30E] PRIMARY KEY ([id])
);
GO


CREATE TABLE [schedule_authority_category] (
    [id] int NOT NULL IDENTITY,
    [category] nvarchar(50) NOT NULL,
    CONSTRAINT [PK__schedule__3213E83F741E93CB] PRIMARY KEY ([id])
);
GO


CREATE TABLE [split_category] (
    [id] int NOT NULL IDENTITY,
    [category] nvarchar(50) NOT NULL,
    [color] nvarchar(7) NULL DEFAULT N'#000000',
    CONSTRAINT [PK__split_ca__3213E83F8B6A2E28] PRIMARY KEY ([id])
);
GO


CREATE TABLE [user] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(32) NOT NULL,
    [birthday] date NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    [google_id] nvarchar(255) NULL,
    [photo] nvarchar(255) NULL,
    [gender] int NULL DEFAULT 0,
    [email] nvarchar(255) NOT NULL,
    [phone] nvarchar(20) NULL,
    [password_hash] nvarchar(255) NOT NULL,
    CONSTRAINT [PK__user__3213E83FEDF2AEDD] PRIMARY KEY ([id])
);
GO


CREATE TABLE [location_category] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [name] nvarchar(50) NOT NULL,
    [color] nvarchar(7) NULL DEFAULT N'#000000',
    CONSTRAINT [PK__location__3213E83F0B4B7B80] PRIMARY KEY ([id]),
    CONSTRAINT [FK_location_category_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [schedule] (
    [id] int NOT NULL IDENTITY,
    [name] nvarchar(100) NOT NULL,
    [start_time] date NOT NULL,
    [end_time] date NOT NULL,
    [user_id] int NOT NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__schedule__3213E83F87F68C02] PRIMARY KEY ([id]),
    CONSTRAINT [FK_schedule_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [search_history] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [search_key] nvarchar(255) NOT NULL,
    [search_date] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__search_h__3213E83FD324566C] PRIMARY KEY ([id]),
    CONSTRAINT [FK_search_history_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [transportation_category] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [name] nvarchar(50) NOT NULL,
    [icon_url] nvarchar(255) NOT NULL,
    CONSTRAINT [PK__transpor__3213E83F07BBE5C4] PRIMARY KEY ([id]),
    CONSTRAINT [FK_transportation_category_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [user_favor] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NULL,
    [favor_category_id] int NULL,
    CONSTRAINT [PK__user_fav__3213E83F43E45F60] PRIMARY KEY ([id]),
    CONSTRAINT [FK_user_favor_favor_category] FOREIGN KEY ([favor_category_id]) REFERENCES [favor_category] ([id]),
    CONSTRAINT [FK_user_favor_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [wishlist] (
    [id] int NOT NULL IDENTITY,
    [user_id] int NOT NULL,
    [name] nvarchar(100) NOT NULL,
    CONSTRAINT [PK__wishlist__3213E83F770F16A6] PRIMARY KEY ([id]),
    CONSTRAINT [FK_wishlists_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [chatroom_chat] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [user_id] int NOT NULL,
    [message] nvarchar(max) NOT NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    [is_focus] bit NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK__chatroom__3213E83F418CA802] PRIMARY KEY ([id]),
    CONSTRAINT [FK_chatroom_chat_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id]),
    CONSTRAINT [FK_chatroom_chat_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [schedule_authority] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [user_id] int NOT NULL,
    [authority_category_id] int NOT NULL,
    CONSTRAINT [PK__schedule__3213E83F68BCF871] PRIMARY KEY ([id]),
    CONSTRAINT [FK_schedule_authority_category] FOREIGN KEY ([authority_category_id]) REFERENCES [schedule_authority_category] ([id]),
    CONSTRAINT [FK_schedule_authority_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id]),
    CONSTRAINT [FK_schedule_authority_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [schedule_group] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [user_id] int NOT NULL,
    [joined_date] datetime NULL DEFAULT ((getdate())),
    [left_date] datetime NULL,
    CONSTRAINT [PK__schedule__3213E83F01543DCD] PRIMARY KEY ([id]),
    CONSTRAINT [FK_schedule_group_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id]),
    CONSTRAINT [FK_schedule_group_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [schedule_preview] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [date] date NOT NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__schedule__3213E83F1D00E400] PRIMARY KEY ([id]),
    CONSTRAINT [FK_schedule_preview_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id])
);
GO


CREATE TABLE [split_expense] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [payer_id] int NOT NULL,
    [split_category_id] int NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [currency_id] int NOT NULL,
    [amount] decimal(10,2) NOT NULL,
    [remark] nvarchar(max) NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__split_ex__3213E83FBE968438] PRIMARY KEY ([id]),
    CONSTRAINT [FK_split_expenses_cost_currency_category] FOREIGN KEY ([currency_id]) REFERENCES [cost_currency_category] ([id]),
    CONSTRAINT [FK_split_expenses_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id]),
    CONSTRAINT [FK_split_expenses_split_category] FOREIGN KEY ([split_category_id]) REFERENCES [split_category] ([id]),
    CONSTRAINT [FK_split_expenses_user_payer] FOREIGN KEY ([payer_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [votes] (
    [id] int NOT NULL IDENTITY,
    [schedule_id] int NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [user_id] int NOT NULL,
    [start_date] datetime NULL DEFAULT ((getdate())),
    [end_date] datetime NOT NULL,
    CONSTRAINT [PK__votes__3213E83FBD512E41] PRIMARY KEY ([id]),
    CONSTRAINT [FK_votes_schedule] FOREIGN KEY ([schedule_id]) REFERENCES [schedule] ([id]),
    CONSTRAINT [FK_votes_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [wishlist_detail] (
    [id] int NOT NULL IDENTITY,
    [wishlist_id] int NOT NULL,
    [location] nvarchar(255) NOT NULL,
    [name] nvarchar(100) NULL,
    [location_category_id] int NULL,
    [created_at] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__wishlist__3213E83FDEE0F11E] PRIMARY KEY ([id]),
    CONSTRAINT [FK_wishlist_detail_location_category] FOREIGN KEY ([location_category_id]) REFERENCES [location_category] ([id]),
    CONSTRAINT [FK_wishlist_detail_wishlists] FOREIGN KEY ([wishlist_id]) REFERENCES [wishlist] ([id])
);
GO


CREATE TABLE [schedule_details] (
    [id] int NOT NULL IDENTITY,
    [schedule_day_id] int NOT NULL,
    [user_id] int NOT NULL,
    [location_name] nvarchar(100) NOT NULL,
    [location] nvarchar(255) NOT NULL,
    [start_time] datetime NOT NULL,
    [end_time] datetime NOT NULL,
    [cost_currency_id] int NULL,
    [cost] decimal(10,2) NULL,
    [remark] nvarchar(max) NULL,
    [is_deleted] bit NULL DEFAULT CAST(0 AS bit),
    [modified_time] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__schedule__3213E83F90422BAE] PRIMARY KEY ([id]),
    CONSTRAINT [FK_schedule_details_cost_currency_category] FOREIGN KEY ([cost_currency_id]) REFERENCES [cost_currency_category] ([id]),
    CONSTRAINT [FK_schedule_details_schedule_preview] FOREIGN KEY ([schedule_day_id]) REFERENCES [schedule_preview] ([id]),
    CONSTRAINT [FK_schedule_details_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [split_expense_participant] (
    [split_expense_id] int NOT NULL,
    [user_id] int NOT NULL,
    [amount] decimal(10,2) NOT NULL,
    [is_paid] bit NULL DEFAULT CAST(0 AS bit),
    CONSTRAINT [PK__split_ex__3DAFFE621DAD9F76] PRIMARY KEY ([split_expense_id], [user_id]),
    CONSTRAINT [FK_split_expense_participant_split_expense] FOREIGN KEY ([split_expense_id]) REFERENCES [split_expense] ([id]),
    CONSTRAINT [FK_split_expense_participant_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id])
);
GO


CREATE TABLE [vote_options] (
    [id] int NOT NULL IDENTITY,
    [vote_id] int NOT NULL,
    [name] nvarchar(100) NOT NULL,
    [wishlist_detail_id] int NULL,
    CONSTRAINT [PK__vote_opt__3213E83FBA9CEB80] PRIMARY KEY ([id]),
    CONSTRAINT [FK_vote_options_votes] FOREIGN KEY ([vote_id]) REFERENCES [votes] ([id]),
    CONSTRAINT [FK_vote_options_wishlist_detail] FOREIGN KEY ([wishlist_detail_id]) REFERENCES [wishlist_detail] ([id])
);
GO


CREATE TABLE [transportation] (
    [id] int NOT NULL IDENTITY,
    [schedule_Details_id] int NOT NULL,
    [transportation_category_id] int NOT NULL,
    [time] datetime NOT NULL,
    [currency_id] int NULL,
    [cost] decimal(10,2) NULL,
    [remark] nvarchar(max) NULL,
    [ticket_image_url] nvarchar(255) NULL,
    CONSTRAINT [PK__transpor__3213E83FF7A18148] PRIMARY KEY ([id]),
    CONSTRAINT [FK_transportation_cost_currency_category] FOREIGN KEY ([currency_id]) REFERENCES [cost_currency_category] ([id]),
    CONSTRAINT [FK_transportation_schedule_details] FOREIGN KEY ([schedule_Details_id]) REFERENCES [schedule_details] ([id]),
    CONSTRAINT [FK_transportation_transportation_category] FOREIGN KEY ([transportation_category_id]) REFERENCES [transportation_category] ([id])
);
GO


CREATE TABLE [vote_result] (
    [id] int NOT NULL IDENTITY,
    [vote_id] int NOT NULL,
    [vote_option_id] int NOT NULL,
    [user_id] int NOT NULL,
    [voted_at] datetime NULL DEFAULT ((getdate())),
    CONSTRAINT [PK__vote_res__3213E83FFF101454] PRIMARY KEY ([id]),
    CONSTRAINT [FK_vote_result_user] FOREIGN KEY ([user_id]) REFERENCES [user] ([id]),
    CONSTRAINT [FK_vote_result_vote_options] FOREIGN KEY ([vote_option_id]) REFERENCES [vote_options] ([id]),
    CONSTRAINT [FK_vote_result_votes] FOREIGN KEY ([vote_id]) REFERENCES [votes] ([id])
);
GO


CREATE INDEX [IX_chatroom_chat_schedule_id] ON [chatroom_chat] ([schedule_id]);
GO


CREATE INDEX [IX_chatroom_chat_user_id] ON [chatroom_chat] ([user_id]);
GO


CREATE UNIQUE INDEX [UQ__cost_cur__357D4CF9165076EB] ON [cost_currency_category] ([code]);
GO


CREATE UNIQUE INDEX [UQ__favor_ca__F7F53CC2FC9AC817] ON [favor_category] ([category]);
GO


CREATE INDEX [IX_location_category_user_id] ON [location_category] ([user_id]);
GO


CREATE INDEX [IX_schedule_user_id] ON [schedule] ([user_id]);
GO


CREATE INDEX [IX_schedule_authority_authority_category_id] ON [schedule_authority] ([authority_category_id]);
GO


CREATE INDEX [IX_schedule_authority_user_id] ON [schedule_authority] ([user_id]);
GO


CREATE UNIQUE INDEX [unique_schedule_id_user_id_authority_category_id] ON [schedule_authority] ([schedule_id], [user_id], [authority_category_id]);
GO


CREATE UNIQUE INDEX [UQ__schedule__F7F53CC224BD2BB8] ON [schedule_authority_category] ([category]);
GO


CREATE INDEX [IX_schedule_details_cost_currency_id] ON [schedule_details] ([cost_currency_id]);
GO


CREATE INDEX [IX_schedule_details_schedule_day_id] ON [schedule_details] ([schedule_day_id]);
GO


CREATE INDEX [IX_schedule_details_user_id] ON [schedule_details] ([user_id]);
GO


CREATE INDEX [IX_schedule_group_user_id] ON [schedule_group] ([user_id]);
GO


CREATE UNIQUE INDEX [unique_schedule_i_user_id] ON [schedule_group] ([schedule_id], [user_id]);
GO


CREATE INDEX [IX_schedule_preview_schedule_id] ON [schedule_preview] ([schedule_id]);
GO


CREATE INDEX [IX_search_history_user_id] ON [search_history] ([user_id]);
GO


CREATE UNIQUE INDEX [UQ__split_ca__F7F53CC28B5562B9] ON [split_category] ([category]);
GO


CREATE INDEX [IX_split_expense_currency_id] ON [split_expense] ([currency_id]);
GO


CREATE INDEX [IX_split_expense_payer_id] ON [split_expense] ([payer_id]);
GO


CREATE INDEX [IX_split_expense_schedule_id] ON [split_expense] ([schedule_id]);
GO


CREATE INDEX [IX_split_expense_split_category_id] ON [split_expense] ([split_category_id]);
GO


CREATE INDEX [IX_split_expense_participant_user_id] ON [split_expense_participant] ([user_id]);
GO


CREATE INDEX [IX_transportation_currency_id] ON [transportation] ([currency_id]);
GO


CREATE INDEX [IX_transportation_schedule_Details_id] ON [transportation] ([schedule_Details_id]);
GO


CREATE INDEX [IX_transportation_transportation_category_id] ON [transportation] ([transportation_category_id]);
GO


CREATE INDEX [IX_transportation_category_user_id] ON [transportation_category] ([user_id]);
GO


CREATE UNIQUE INDEX [UQ__user__AB6E6164A96D0EA6] ON [user] ([email]);
GO


CREATE UNIQUE INDEX [UQ__user__B43B145F4F51B9C5] ON [user] ([phone]) WHERE [phone] IS NOT NULL;
GO


CREATE INDEX [IX_user_favor_favor_category_id] ON [user_favor] ([favor_category_id]);
GO


CREATE UNIQUE INDEX [unique_user_id_favor_category_id] ON [user_favor] ([user_id], [favor_category_id]) WHERE [user_id] IS NOT NULL AND [favor_category_id] IS NOT NULL;
GO


CREATE INDEX [IX_vote_options_vote_id] ON [vote_options] ([vote_id]);
GO


CREATE INDEX [IX_vote_options_wishlist_detail_id] ON [vote_options] ([wishlist_detail_id]);
GO


CREATE INDEX [IX_vote_result_user_id] ON [vote_result] ([user_id]);
GO


CREATE INDEX [IX_vote_result_vote_option_id] ON [vote_result] ([vote_option_id]);
GO


CREATE UNIQUE INDEX [unique_vote_id_vote_option_id_user_id] ON [vote_result] ([vote_id], [vote_option_id], [user_id]);
GO


CREATE INDEX [IX_votes_schedule_id] ON [votes] ([schedule_id]);
GO


CREATE INDEX [IX_votes_user_id] ON [votes] ([user_id]);
GO


CREATE INDEX [IX_wishlist_user_id] ON [wishlist] ([user_id]);
GO


CREATE INDEX [IX_wishlist_detail_location_category_id] ON [wishlist_detail] ([location_category_id]);
GO


CREATE INDEX [IX_wishlist_detail_wishlist_id] ON [wishlist_detail] ([wishlist_id]);
GO