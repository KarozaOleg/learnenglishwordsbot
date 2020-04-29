CREATE TABLE public.words_learn_set
(
    id SERIAL PRIMARY KEY,
    id_word int,
    id_learn_set int,
    FOREIGN KEY (id_word) REFERENCES words (id) ON UPDATE CASCADE ON DELETE NO ACTION,
    FOREIGN KEY (id_learn_set) REFERENCES learn_set (id) ON UPDATE CASCADE ON DELETE NO ACTION
);

CREATE UNIQUE INDEX id_word_and_id_learn_set ON words_learn_set (id_word, id_learn_set);