(() => {
    class SupabaseTableSignaling {
        constructor(config, logger, options = {}) {
            if (!window.supabase?.createClient) {
                throw new Error("Supabase JS SDK chưa được tải.");
            }

            this.config = config;
            this.log = logger ?? (() => { });
            this.client = window.supabase.createClient(config.supabaseUrl, config.supabaseAnonKey, {
                auth: { persistSession: false, autoRefreshToken: false }
            });
            this.tableName = "classroom_webrtc_signals";
            this.channel = null;
            this.handlers = new Set();
            this.seenSignalIds = new Set();
            this.joinedAt = options.joinedAt || new Date().toISOString();
        }

        get roomId() {
            return this.config.roomId;
        }

        get senderId() {
            return String(this.config.userId);
        }

        async start() {
            if (this.channel) {
                return;
            }

            this.channel = this.client
                .channel(`courseguard-webrtc-${this.roomId}-${this.senderId}-${Date.now()}`)
                .on(
                    "postgres_changes",
                    {
                        event: "INSERT",
                        schema: "public",
                        table: this.tableName,
                        filter: `room_id=eq.${this.roomId}`
                    },
                    (payload) => this.#handleSignal(payload.new)
                );

            const status = await this.channel.subscribe();
            this.log(`Supabase Realtime: ${status}`);
        }

        async stop() {
            if (!this.channel) {
                return;
            }

            await this.client.removeChannel(this.channel);
            this.channel = null;
            this.handlers.clear();
            this.seenSignalIds.clear();
        }

        onSignal(handler) {
            this.handlers.add(handler);
            return () => this.handlers.delete(handler);
        }

        async send(type, payload = {}, receiverId = null) {
            const row = {
                room_id: this.roomId,
                session_id: this.config.sessionId,
                sender_id: this.senderId,
                sender_role: this.config.role,
                receiver_id: receiverId === null || receiverId === undefined ? null : String(receiverId),
                signal_type: type,
                payload,
                created_at: new Date().toISOString()
            };

            const { error } = await this.client.from(this.tableName).insert(row);
            if (error) {
                throw new Error(`Không gửi được signaling '${type}': ${error.message}`);
            }
        }

        #handleSignal(row) {
            if (!row || this.seenSignalIds.has(row.id)) {
                return;
            }

            if (row.created_at && new Date(row.created_at).getTime() < new Date(this.joinedAt).getTime()) {
                return;
            }

            this.seenSignalIds.add(row.id);

            if (String(row.sender_id) === this.senderId) {
                return;
            }

            if (row.receiver_id && String(row.receiver_id) !== this.senderId) {
                return;
            }

            for (const handler of this.handlers) {
                void handler(row);
            }
        }
    }

    window.CourseGuardSupabaseTableSignaling = SupabaseTableSignaling;
})();
