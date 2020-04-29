CREATE TABLE public.taskstolearn
(
    id SERIAL PRIMARY KEY,
    id_user integer NOT NULL,
    id_word integer NOT NULL,
    is_revers boolean NOT NULL,
    sended boolean NOT NULL DEFAULT(false),
    amount_wrong_answer integer NOT NULL DEFAULT(0)
)