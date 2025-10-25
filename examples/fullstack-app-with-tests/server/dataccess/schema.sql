drop schema if exists library cascade;
create schema if not exists library;

create table library.author
(
    id        text primary key not null,
    name      text             not null,
    createdAt timestamp with time zone not null
);

create table library.genre
(
    id        text primary key not null,
    name      text             not null,
    createdAt timestamp with time zone not null
);

create table library.book
(
    id        text primary key not null,
    title     text             not null,
    pages     int              not null,
    createdAt timestamp with time zone not null,
    genreId   text             references library.genre (id) on delete set null
);

create table library.authorbookjunction
(
    authorId text references library.author (id) on delete cascade,
    bookId   text references library.book (id) on delete cascade,
    primary key (authorId, bookId)
);