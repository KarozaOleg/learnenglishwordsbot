CREATE TABLE public.words
(
    id SERIAL PRIMARY KEY,
    russian text,
    english text unique
);