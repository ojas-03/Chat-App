-- ==============================================================================
-- E2EE PREKEY TABLES (Signal Protocol Support)
-- Run this against your existing PostgreSQL chatapp database
-- ==============================================================================

CREATE TABLE IF NOT EXISTS chatapp.prekey_bundles (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    user_id UUID NOT NULL REFERENCES chatapp.users(id) ON DELETE CASCADE,
    identity_public_key TEXT NOT NULL,
    signed_prekey TEXT NOT NULL,
    signed_prekey_signature TEXT NOT NULL,
    signed_prekey_id INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT CURRENT_TIMESTAMP,
    CONSTRAINT uq_prekey_bundle_user UNIQUE(user_id)
);

CREATE INDEX IF NOT EXISTS idx_prekey_bundles_user ON chatapp.prekey_bundles (user_id);

CREATE TABLE IF NOT EXISTS chatapp.one_time_prekeys (
    id UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    bundle_id UUID NOT NULL REFERENCES chatapp.prekey_bundles(id) ON DELETE CASCADE,
    key_id INT NOT NULL,
    public_key TEXT NOT NULL,
    is_consumed BOOLEAN NOT NULL DEFAULT FALSE,
    consumed_at TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS idx_otp_bundle ON chatapp.one_time_prekeys (bundle_id);
CREATE INDEX IF NOT EXISTS idx_otp_unconsumed ON chatapp.one_time_prekeys (bundle_id, is_consumed) WHERE NOT is_consumed;
