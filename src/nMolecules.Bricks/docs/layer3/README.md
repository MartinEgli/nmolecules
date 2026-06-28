# NMolecules.Bricks — Use Cases by Layer

Status: June 27, 2026

This folder collects use-case documents separated by Bricks layer.

- Layer 1:
  [bricks-layer1-core-use-cases.md](../layer1/bricks-layer1-core-use-cases.md)
- Layer 2:
  [bricks-layer2-building-block-use-cases.md](../layer2/bricks-layer2-building-block-use-cases.md)
- Layer 3 overview: [bricks-layer3-usecases.md](./bricks-layer3-usecases.md)
- Layer 3 DDD: [bricks-layer3-usecases-ddd.md](./bricks-layer3-usecases-ddd.md)
- Layer 3 Clean Architecture:
  [bricks-layer3-usecases-clean-architecture.md](./bricks-layer3-usecases-clean-architecture.md)
- Layer 3 Hexagonal:
  [bricks-layer3-usecases-hexagonal.md](./bricks-layer3-usecases-hexagonal.md)

Guideline:

- Layer 1 use cases use only generic core concepts.
- Layer 2 use cases use reusable building blocks defined on top of Layer 1.
- Layer 3 use cases show concrete architectural scenarios built from Layer 2.

## Layer Placement Decision

Clean Architecture, Hexagonal Architecture and DDD intentionally appear in two
places:

| Layer | Ownership |
| --- | --- |
| Layer 2 | Reusable packs: role catalogs, default rules, constraints and profile definitions that can be imported by many projects. |
| Layer 3 | Use cases: concrete architecture scenarios that apply those packs to real project shapes and show expected pass/violation behavior. |

So `bricks-layer2-building-blocks.md` owns the reusable Clean Architecture and
Hexagonal packs, while this folder owns the Clean Architecture, Hexagonal and
DDD scenario documents. Moving the scenario documents back into Layer 2 would
mix reusable package design with onboarding examples; moving the pack
definitions into Layer 3 would hide reusable API intent inside sample stories.
