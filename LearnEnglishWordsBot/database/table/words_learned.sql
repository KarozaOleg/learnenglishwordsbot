CREATE TABLE words_learned
(
    id SERIAL PRIMARY KEY,
    id_user integer not null,
    id_word integer not null,
    amount_success_answers integer default(0) not null,
    FOREIGN KEY (id_user) REFERENCES users (id) ON UPDATE CASCADE ON DELETE NO ACTION,
    FOREIGN KEY (id_word) REFERENCES words (id) ON UPDATE CASCADE ON DELETE NO ACTION    
);

CREATE UNIQUE INDEX id_user_and_id_word ON words_learned (id_user, id_word);