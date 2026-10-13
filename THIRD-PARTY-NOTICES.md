# Third-Party Notices and Attribution

This repository began from course material by **Engin Demiroğ** and contains
inherited architectural and application-foundation code. The source is kept in
the repository so reviewers can inspect how the building blocks are used.

## Course Reference

The local course project used while developing this portfolio is mirrored at
[`realfeanor/nArchitectureDemo`](https://github.com/realfeanor/nArchitectureDemo/tree/518c4b1c8b5fe753cf1ff49be5a98b55a06957f5),
pinned here to commit `518c4b1c8b5fe753cf1ff49be5a98b55a06957f5`.

The course architecture is published by Kodlama.io in the following upstream
repositories:

- [`kodlamaio-projects/nArchitecture`](https://github.com/kodlamaio-projects/nArchitecture)
  — MIT License.
- [`kodlamaio-projects/nArchitecture.Core`](https://github.com/kodlamaio-projects/nArchitecture.Core)
  — MIT License.

The copyright and license notices in those upstream repositories continue to
apply to material derived from them. This notice is attribution, not a claim
that every file in this portfolio is copied from those repositories.

## Inherited Foundation

The inherited portion includes `CorePackages`, the initial Clean Architecture
solution structure, identity/authentication features, and reusable application
behaviors such as validation, authorization, caching, logging, and localization.

## Original Portfolio Work

The event-reservation domain and its venue, event, session, seat-hold,
confirmation, cancellation, and expiration workflows were developed for this
portfolio. That work includes domain rules, persistence mappings and migrations,
HTTP endpoints, concurrency handling, health and correlation behavior, automated
tests, architecture checks, and project documentation.

NuGet dependencies remain subject to their own package licenses. The versions
of AutoMapper and MediatR currently referenced use Lucky Penny Software
licensing. The project owner has registered for the applicable Community
License; its key is supplied at runtime and is never stored in this repository.
