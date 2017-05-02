// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal
{
    public class SharedTableConvention :
        IEntityTypeConvention,
        IEntityTypeAnnotationSetConvention,
        IForeignKeyOwnershipConvention,
        IForeignKeyUniquenessConvention
    {
        public SharedTableConvention([NotNull] IRelationalAnnotationProvider annotationProvider)
        {
            AnnotationProvider = annotationProvider;
        }

        protected virtual IRelationalAnnotationProvider AnnotationProvider { get; }

        public virtual InternalEntityTypeBuilder Apply(InternalEntityTypeBuilder entityTypeBuilder)
        {
            var ownership = entityTypeBuilder.Metadata.GetForeignKeys().SingleOrDefault(fk => fk.IsOwnership && fk.IsUnique);
            if (ownership != null)
            {
                SetOwnedTable(ownership);
            }

            return entityTypeBuilder;
        }

        public virtual Annotation Apply(
            InternalEntityTypeBuilder entityTypeBuilder, string name, Annotation annotation, Annotation oldAnnotation)
        {
            var entityType = entityTypeBuilder.Metadata;
            var providerAnnotations = (AnnotationProvider.For(entityType) as RelationalEntityTypeAnnotations)?.ProviderFullAnnotationNames;
            if (name == RelationalFullAnnotationNames.Instance.TableName
                || name == RelationalFullAnnotationNames.Instance.Schema
                || (providerAnnotations != null
                    && (name == providerAnnotations.TableName
                        || name == providerAnnotations.Schema)))
            {
                foreach (var foreignKey in entityType.GetReferencingForeignKeys())
                {
                    if (foreignKey.IsOwnership
                        && foreignKey.IsUnique)
                    {
                        SetOwnedTable(foreignKey);
                    }
                }
            }

            return annotation;
        }

        public virtual InternalRelationshipBuilder Apply(InternalRelationshipBuilder relationshipBuilder)
        {
            var foreignKey = relationshipBuilder.Metadata;
            if (foreignKey.IsOwnership
                && foreignKey.IsUnique)
            {
                SetOwnedTable(foreignKey);
            }
            else
            {
                // TODO: Restore previous value
                foreignKey.DeclaringEntityType.Builder.Relational(ConfigurationSource.Convention)
                    .ToTable(null, null);
            }

            return relationshipBuilder;
        }

        private void SetOwnedTable(ForeignKey foreignKey)
        {
            var ownerType = foreignKey.PrincipalEntityType;
            foreignKey.DeclaringEntityType.Builder.Relational(ConfigurationSource.Convention)
                .ToTable(AnnotationProvider.For(ownerType).TableName, AnnotationProvider.For(ownerType).Schema);
        }
    }
}
