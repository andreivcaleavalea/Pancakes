using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BlogService.Data;
using BlogService.Models.Entities;

namespace BlogService.Data.Seeders;

public static class BlogDataSeeder
{
    public static async Task SeedAsync(BlogDbContext context)
    {
        if (context.BlogPosts.Any())
            return;

        var authorId = Guid.NewGuid();
        var now = DateTime.UtcNow;
        
        var itPosts = new List<(string title, string content, string featuredImage)>
        {
            (
                "Introduction to ASP.NET Core 9: Features That Transform Web Development",
                "ASP.NET Core 9 brings significant improvements in performance and productivity. In this article, we explore enhanced Minimal APIs, new Blazor features, and Docker container optimizations. We'll see how Native AOT compilation reduces startup time by up to 50% and how new middleware simplifies authentication and authorization in modern applications. The framework continues to evolve with cloud-native development in mind, offering better integration with containerized environments and microservices architectures.",
                "https://images.unsplash.com/photo-1461749280684-dccba630e2f6?w=800&h=400&fit=crop"
            ),
            (
                "Migrating from React to Vue.js: A Practical Guide for Teams",
                "Migrating a complex application from React to Vue.js might seem intimidating, but with the right strategy, it becomes much easier. This guide covers migration planning, comparison between React Hooks and Vue Composition API, state management with Pinia vs Redux, and best practices for a smooth transition. We include code examples and a complete checklist for migration, ensuring your team can make informed decisions about when and how to migrate.",
                "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800&h=400&fit=crop"
            ),
            (
                "Kubernetes in Production: Cost and Performance Optimization",
                "Running Kubernetes in production comes with unique challenges related to costs and performance. We analyze strategies for resource quotas, horizontal pod autoscaling, and vertical pod autoscaling. We explore monitoring tools (Prometheus, Grafana), logging (ELK Stack), and how to implement efficient CI/CD pipelines with GitOps. Bonus: reduce costs by up to 40% through resource optimization and proper cluster management.",
                "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&h=400&fit=crop"
            ),
            (
                "API Design with FastAPI: Performance and Type Safety in Python",
                "FastAPI revolutionizes API development in Python by combining performance with type safety. We explore async/await patterns, automatic API documentation with OpenAPI, dependency injection, and custom middleware. We compare performance with Flask and Django REST Framework, and see how to implement JWT authentication, rate limiting, and robust error handling for production-ready APIs.",
                "https://images.unsplash.com/photo-1526379095098-d400fd0bf935?w=800&h=400&fit=crop"
            ),
            (
                "Machine Learning in the Browser: TensorFlow.js vs ONNX.js",
                "Implementing machine learning directly in the browser is becoming increasingly popular. We compare TensorFlow.js with ONNX.js for client-side inference, analyzing performance, bundle size, and ideal use cases. We include practical implementations for image classification, natural language processing, and real-time object detection using WebGL acceleration for optimal performance.",
                "https://images.unsplash.com/photo-1555255707-c07966088b7b?w=800&h=400&fit=crop"
            ),
            (
                "Microservices with .NET: Event Sourcing and CQRS Patterns",
                "Event Sourcing and Command Query Responsibility Segregation (CQRS) are essential patterns for complex microservices. We explore their implementation in .NET using MediatR, EventStore, and Apache Kafka for event streaming. We analyze consistency challenges, eventual consistency, and saga patterns for distributed transactions in large-scale systems.",
                "https://images.unsplash.com/photo-1518709268805-4e9042af2176?w=800&h=400&fit=crop"
            ),
            (
                "PostgreSQL Performance Tuning: From Slow Queries to Advanced Optimization",
                "PostgreSQL offers powerful tools for performance optimization, but they need to be understood correctly. We analyze query planning, index strategies (B-tree, GIN, GiST), partitioning, and connection pooling. We explore pg_stat_statements for monitoring, VACUUM and ANALYZE for maintenance, and parameter configuration for specific workloads.",
                "https://images.unsplash.com/photo-1544383835-bda2bc66a55d?w=800&h=400&fit=crop"
            ),
            (
                "Modern CSS: Container Queries and Cascade Layers in 2025",
                "CSS continues to evolve rapidly with features that change how we think about responsive design. Container Queries allow styling based on container size, not viewport size. Cascade Layers offer explicit control over specificity. We also explore native CSS Nesting, :has() selector, and color-mix() function for modern design systems.",
                "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=800&h=400&fit=crop"
            ),
            (
                "GraphQL Federation: API Scalability in Distributed Architectures",
                "GraphQL Federation allows combining multiple GraphQL services into a single API gateway. We explore Apollo Federation vs Schema Stitching, subgraph implementation, and strategies for schema evolution. We analyze security, caching, and monitoring challenges in federated graphs, plus patterns for team autonomy in large organizations.",
                "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=400&fit=crop"
            ),
            (
                "Rust for Backend Development: Why It's Gaining Ground in 2025",
                "Rust is becoming increasingly popular for backend development due to its performance and memory safety. We explore the Rust ecosystem for web development: Actix-web, Axum, and Rocket for web servers, SQLx for database access, and Tokio for async programming. We compare with Go and Node.js in terms of performance and developer experience.",
                "https://images.unsplash.com/photo-1555066931-4365d14bab8c?w=800&h=400&fit=crop"
            ),
            (
                "CI/CD with GitHub Actions: Advanced Workflows for Modern Applications",
                "GitHub Actions offers enormous flexibility for CI/CD pipelines. We explore matrix builds for multiple environments, self-hosted runners for performance, and security best practices with secrets management. We implement deployment strategies (blue-green, canary), parallel testing, and integration with cloud providers for infrastructure as code.",
                "https://images.unsplash.com/photo-1556075798-4825dfaaf498?w=800&h=400&fit=crop"
            ),
            (
                "WebAssembly in 2025: Practical Use Cases and Limitations",
                "WebAssembly (WASM) goes beyond the concept of 'JavaScript killer' and becomes a valuable tool for specific use cases. We analyze performance for intensive computations, porting legacy C/C++ code, and integration with modern JavaScript frameworks. We explore WASI for server-side WASM and debugging and profiling challenges.",
                "https://images.unsplash.com/photo-1517077304055-6e89abbf09b0?w=800&h=400&fit=crop"
            ),
            (
                "Database Sharding with MongoDB: Strategies for Large-Scale Applications",
                "Sharding enables horizontal scaling of MongoDB for large data volumes. We explore shard key selection (compound vs hashed), chunk migration strategies, and automated balancing. We analyze query routing challenges, transactions in sharded clusters, and monitoring for hotspot identification and data distribution optimization.",
                "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&h=400&fit=crop"
            ),
            (
                "React Server Components: The Future of React Development",
                "React Server Components fundamentally change how we think about rendering in React. We explore differences from traditional SSR, streaming capabilities, and integration with Next.js App Router. We analyze patterns for data fetching, cache invalidation, and hybrid client-server component trees for performant applications.",
                "https://images.unsplash.com/photo-1633356122544-f134324a6cee?w=800&h=400&fit=crop"
            ),
            (
                "Edge Computing with Cloudflare Workers: Global Performance for Web Apps",
                "Cloudflare Workers enables code execution at the network edge for minimal latency. We explore V8 isolates vs traditional containers, patterns for edge-side rendering, and integration with storage solutions (KV, Durable Objects, R2). We implement A/B testing, personalization, and API optimization globally with cold start times under 5ms.",
                "https://images.unsplash.com/photo-1451187580459-43490279c0fa?w=800&h=400&fit=crop"
            ),
            (
                "TypeScript 5.4: New Features and Best Practices for Large Codebases",
                "TypeScript 5.4 brings significant improvements for developer productivity. We explore preserveValueImports, satisfies operator for type narrowing, and const type parameters. We analyze strategies for migration in large codebases, performance tuning for compilation times, and advanced patterns for type-safe APIs with conditional types.",
                "https://images.unsplash.com/photo-1516259762381-22954d7d3ad2?w=800&h=400&fit=crop"
            ),
            (
                "Monitoring and Observability: OpenTelemetry in Distributed Applications",
                "OpenTelemetry standardizes telemetry data collection for modern applications. We explore automatic vs manual instrumentation, correlation between traces, metrics, and logs. We implement distributed tracing with Jaeger, metrics collection with Prometheus, and log aggregation with OpenSearch. We analyze strategies for sampling and cost optimization.",
                "https://images.unsplash.com/photo-1551288049-bebda4e38f71?w=800&h=400&fit=crop"
            ),
            (
                "Terraform and Infrastructure as Code: Patterns for Large Teams",
                "Terraform enables infrastructure management as code, but at the scale of a large team, it requires specific patterns. We explore module organization, state management with remote backends, and CI/CD integration for infrastructure changes. We analyze strategies for environment promotion, secret management, and collaboration workflows with Terraform Cloud.",
                "https://images.unsplash.com/photo-1558494949-ef010cbdcc31?w=800&h=400&fit=crop"
            ),
            (
                "Flutter for Web: Performance and Limitations in 2025",
                "Flutter Web has evolved significantly but still has specific limitations compared to native applications. We analyze CanvasKit vs HTML rendering engines, SEO challenges, and bundle size optimization. We explore integration with web APIs, PWA capabilities, and when to choose Flutter Web vs dedicated web development frameworks.",
                "https://images.unsplash.com/photo-1512941937669-90a1b58e7e9c?w=800&h=400&fit=crop"
            ),
            (
                "Security in Cloud Native Applications: Zero Trust Architecture",
                "Zero Trust Architecture becomes the standard for security in cloud native applications. We explore identity verification, principle of least privilege, and continuous monitoring. We implement mutual TLS (mTLS), service mesh security with Istio, and secrets management with HashiCorp Vault. We analyze compliance automation and security scanning in CI/CD pipelines.",
                "https://images.unsplash.com/photo-1563013544-824ae1b704d3?w=800&h=400&fit=crop"
            )
        };

        var posts = new List<BlogPost>();
        for (int i = 0; i < itPosts.Count; i++)
        {
            var (title, content, featuredImage) = itPosts[i];
            posts.Add(new BlogPost
            {
                Id = Guid.NewGuid(),
                Title = title,
                Content = content,
                FeaturedImage = featuredImage,
                Status = PostStatus.Published,
                AuthorId = authorId,
                CreatedAt = now.AddDays(-(i + 1)),
                UpdatedAt = now.AddDays(-(i + 1)),
                PublishedAt = now.AddDays(-(i + 1))
            });
        }
        await context.BlogPosts.AddRangeAsync(posts);
        await context.SaveChangesAsync();
    }
}
